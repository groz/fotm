namespace FotM.Athena

open System
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.CollectionExtensions
open FotM.Hephaestus.Math
open NodaTime
open Microsoft.ServiceBus.Messaging

type UpdateValidationResult =
| OutdatedUpdate
| DuplicateUpdate
| ExcessiveUpdate
| ValidUpdate

module Athena =
    open FotM.Hephaestus

    (*
        1. calculate updates/diffs
        2. split into non-intersecting groups
        3. find best k-partition for each group, i.e. teams for each group
        4. merge teams
        5. post update
    *)

    let duplicateCheckPeriod = Duration.FromMinutes(20L)

    let logAthenaEvent (snapshot: LadderSnapshot<PlayerEntry>) (label: string) (value: string) = 
        GoogleAnalytics.sendEvent "UA-49247455-4" "Athena" {
            category = snapshot.region + "_athena_event"
            action = snapshot.bracket.url
            label = label
            value = value
        } |> ignore

    let calcUpdates currentSnapshot previousSnapshot =
        let previousMap = previousSnapshot.ladder |> Array.map (fun e -> e.player, e) |> Map.ofArray

        currentSnapshot.ladder
        |> Seq.map (fun current -> 
            match previousMap.TryFind(current.player) with
            | None -> None
            | Some(previous) -> 
                if current.seasonTotal <= previous.seasonTotal then
                    None
                else
                    logInfo "%A updated to %A" previous current
                    Some(current - previous)
            )
        |> Seq.choose id
        |> Seq.toList

    let split(updates: PlayerUpdate list): PlayerUpdate list list =
        // 2. split into non-intersecting groups
        let horde, alliance = updates |> List.partition (fun u -> u.player.faction = Faction.Horde)
        
        let hw, hl = horde |> List.partition (fun u -> u.ratingDiff > 0)
        let aw, al = alliance |> List.partition (fun u -> u.ratingDiff > 0)

        [hw; hl; aw; al] |> List.filter (fun g -> not g.IsEmpty)

    let featureExtractor (pu: PlayerUpdate) : Vector =
        [|
            float pu.ratingDiff
            float pu.ranking
            float pu.rating
            float pu.weeklyWins
            float pu.weeklyLosses
            float pu.seasonWins
            float pu.seasonLosses
        |]  

    let findTeamsInGroup (teamSize) (snapshotTime: NodaTime.Instant) (updateGroup: PlayerUpdate list) =
        let g = updateGroup |> Array.ofList

        if g.Length < teamSize then
            []
        else
            let clusterer = AthenaKMeans(featureExtractor, true, true, teamSize)
            let nGroups = int( ceil (g.Length ./. teamSize) )
            let clustering = clusterer.computeGroups g nGroups
            
            clustering
            |> Seq.mapi (fun i ci -> ci, g.[i])
            |> toMultiMap
            |> Map.toList
            |> List.map (fun (i, updateList) -> updateList |> Teams.createEntry snapshotTime)

    let findTeams snapshot previousSnapshot =
        let updates = calcUpdates snapshot previousSnapshot
        logInfo "[%s, %s] Total players updated: %i" snapshot.region snapshot.bracket.url updates.Length

        let groups = split updates

        for g in groups do
            logInfo "(-- [%s, %s] group: %A --)" snapshot.region snapshot.bracket.url g

        let teams = groups 
                    |> List.collect (findTeamsInGroup snapshot.bracket.teamSize snapshot.timeTaken)

        let coverage = (float (teams.Length * snapshot.bracket.teamSize)) / (float updates.Length)
        logAthenaEvent snapshot "update_coverage" (string coverage)

        teams

    let isCurrent snapshot =
        let elapsed = SystemClock.Instance.Now - snapshot.timeTaken
        elapsed < duplicateCheckPeriod

    /// <summary>Days passed since last seen < total number of times seen.</summary>
    let seenOften (teamEntries: TeamEntry array) =
        let lastEntry = teamEntries |> Array.maxBy(fun t -> t.snapshotTime)
        let lastSeen = lastEntry.snapshotTime.ToDateTimeUtc()

        let totalTimesSeen = teamEntries.Length
        let totalDays = (DateTime.UtcNow - lastSeen).TotalDays

        totalTimesSeen ./ totalDays > 1.0

    let calculateLadder (teamHistory: TeamEntry list) =
        teamHistory
        |> Seq.groupBy (fun teamEntry -> teamEntry.players)
        |> Seq.map (fun (players, teamEntries) -> players, teamEntries |> Array.ofSeq)
        |> Seq.filter (fun (players, teamEntries) -> teamEntries |> seenOften)
        |> Seq.map (fun (players, teamEntries) -> teamEntries |> Teams.createTeamInfo)
        |> List.ofSeq

    let validateUpdate currentSnapshot previousSnapshot =
        let previousMap = previousSnapshot.ladder |> Array.map (fun e -> e.player, e) |> Map.ofArray

        let updatedPairs = 
            currentSnapshot.ladder
            |> Seq.map (fun current -> 
                match previousMap.TryFind(current.player) with
                | Some(previous) when current.seasonTotal <> previous.seasonTotal -> 
                    Some(current, previous) //Some(current.seasonTotal - previous.seasonTotal)
                | _ -> None
            )
            |> Seq.choose id
            |> Seq.toList

        let ok, outdated = updatedPairs |> List.partition (fun (c, p) -> c.seasonTotal - p.seasonTotal > 0)

        if outdated.Length > 0 then 
            logInfo "[%s, %s] outdated update: %A" currentSnapshot.region currentSnapshot.bracket.url outdated 
            OutdatedUpdate
        else 
            let normal, excessive = ok |> List.partition (fun (c, p) -> c.seasonTotal - p.seasonTotal = 1)

            if excessive.Length > 0 then 
                logInfo "[%s, %s] excessive update: %A" currentSnapshot.region currentSnapshot.bracket.url excessive
                ExcessiveUpdate
            else if normal.Length > 0 || currentSnapshot.ladder <> previousSnapshot.ladder then ValidUpdate
            else
                logInfo "[%s, %s] duplicate update" currentSnapshot.region currentSnapshot.bracket.url
                DuplicateUpdate

    let isValid (t: TeamEntry) =
        (not (isNull t.players)) && t.players.Length <> 0

    let sendUpdate newTeamHistory region bracket (storage: Storage) (historyStorage: Storage) (updatePublisher: TopicWrapper) =
        let teamLadder = calculateLadder newTeamHistory
                    
        let ladderUrl = storage.upload (teamLadder)

        let savedUri = historyStorage.upload(newTeamHistory)
        logInfo "Current history uploaded to %A" savedUri

        updatePublisher.send {
            storageLocation = ladderUrl
            region = region
            bracket = bracket
        }

    let processUpdate snapshot snapshotHistory teamHistory (storage: Storage) (updatePublisher: TopicWrapper) (historyStorage: Storage) =
        let filteredLadder = snapshot.ladder |> Seq.distinctBy (fun p -> p.player) |> Seq.toArray // discards later entries
        let snapshot = { snapshot with ladder = filteredLadder }
        
        let currentSnapshotHistory = snapshotHistory |> List.filter isCurrent

        if currentSnapshotHistory |> List.exists (fun entry -> entry.ladder = snapshot.ladder) then
            logInfo "[%s, %s] Snapshot found in history. Skipping..." snapshot.region snapshot.bracket.url
            currentSnapshotHistory, teamHistory
        else
            match currentSnapshotHistory with
            | previousSnapshot :: tail  ->

                match validateUpdate snapshot previousSnapshot with
                | ValidUpdate ->
                    logAthenaEvent snapshot "calculation" "valid"

                    let teams = 
                        findTeams snapshot previousSnapshot
                        |> List.filter (fun t -> t.players.Length = snapshot.bracket.teamSize)

                    for team in teams do 
                        logInfo "<<< [%s, %s] Team found: %A >>>" snapshot.region snapshot.bracket.url team

                    let newTeamHistory = (teams @ teamHistory) |> List.filter isValid

                    logAthenaEvent snapshot "teams_found" (string teams.Length)

                    if teams.Length <> 0 then
                        logInfo "[%s, %s] Posting ladder update..." snapshot.region snapshot.bracket.url
                        sendUpdate newTeamHistory snapshot.region snapshot.bracket storage historyStorage updatePublisher
                    else
                        logInfo "[%s, %s] No new teams found." snapshot.region snapshot.bracket.url
                    
                    snapshot :: currentSnapshotHistory, newTeamHistory
                | DuplicateUpdate -> 
                    logAthenaEvent snapshot "calculation" "duplicate"
                    logInfo "[%s, %s] Duplicate update. Skipping..." snapshot.region snapshot.bracket.url
                    currentSnapshotHistory, teamHistory
                | OutdatedUpdate ->
                    logAthenaEvent snapshot "calculation" "outdated"
                    logInfo "[%s, %s] Outdated update. Skipping..." snapshot.region snapshot.bracket.url
                    currentSnapshotHistory, teamHistory
                | ExcessiveUpdate ->
                    logAthenaEvent snapshot "calculation" "excessive"
                    logInfo "[%s, %s] Excessive update. Skipping..." snapshot.region snapshot.bracket.url
                    snapshot :: currentSnapshotHistory, teamHistory
            | _ -> 
                logInfo "%s, %s Snapshot history is empty, couldn't calculate updates. Added snapshot to history." snapshot.region snapshot.bracket.url
                snapshot :: currentSnapshotHistory, teamHistory