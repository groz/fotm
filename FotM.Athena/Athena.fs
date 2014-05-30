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
    (*
        1. calculate updates/diffs
        2. split into non-intersecting groups
        3. find best k-partition for each group, i.e. teams for each group
        4. merge teams
        5. post update
    *)

    let duplicateCheckPeriod = Duration.FromHours(1L)

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
            pu.ratingDiff
            pu.ranking
            pu.rating
            pu.weeklyWins
            pu.weeklyLosses
            pu.seasonWins
            pu.seasonLosses
        |]  
        |> Array.map float

    let findTeamsInGroup (teamSize) (snapshotTime: NodaTime.Instant) (updateGroup: PlayerUpdate list) =
        let g = updateGroup |> Array.ofList

        if g.Length < teamSize then
            []
        else
            let clusterer = AthenaKMeans(featureExtractor, true, true)
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

        groups 
        |> List.fold (fun acc g -> acc @ findTeamsInGroup snapshot.bracket.teamSize snapshot.timeTaken g) []

    let isCurrent snapshot =
        (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let seenOften (teamEntries: TeamEntry seq) =
        let firstEntry = teamEntries |> Seq.minBy(fun t -> t.snapshotTime)
        let lastEntry = teamEntries |> Seq.maxBy(fun t -> t.snapshotTime)
        let totalTimesSeen = teamEntries |> Seq.length
        let firstTime = firstEntry.snapshotTime.ToDateTimeUtc()
        let lastTime = lastEntry.snapshotTime.ToDateTimeUtc()
        let totalDays = (lastTime.Date - firstTime.Date).TotalDays |> int

        if (totalDays > 0) then
            totalTimesSeen ./. totalDays > 1.0
        else
            totalTimesSeen > 1

    let calculateLadder (teamHistory: TeamEntry list) =
        teamHistory
        |> Seq.groupBy (fun teamEntry -> teamEntry.players)
        |> Seq.filter (fun (players, teamEntries) -> teamEntries |> seenOften) // confidence check
        |> Seq.map (fun (players, teamEntries) -> teamEntries |> Teams.createTeamInfo)
        |> List.ofSeq

    let validateUpdate currentSnapshot previousSnapshot =
        let previousMap = previousSnapshot.ladder |> Array.map (fun e -> e.player, e) |> Map.ofArray

        let updatedPairs = 
            currentSnapshot.ladder
            |> Seq.map (fun current -> 
                match previousMap.TryFind(current.player) with
                | Some(previous) when current.seasonTotal <> previous.seasonTotal -> 
                    Some(current.seasonTotal - previous.seasonTotal)
                | _ -> None
            )
            |> Seq.choose id
            |> Seq.toList

        let outdated, ok = updatedPairs |> List.partition ((>) 0)

        if outdated.Length > 0 then 
            logInfo "[%s, %s] outdated update: %A" currentSnapshot.region currentSnapshot.bracket.url outdated 
            OutdatedUpdate
        else 
            let normal, excessive = ok |> List.partition ((=) 1)

            if excessive.Length > 0 then 
                logInfo "[%s, %s] excessive update: %A" currentSnapshot.region currentSnapshot.bracket.url excessive
                ExcessiveUpdate
            else if normal.Length > 0 || currentSnapshot.ladder <> previousSnapshot.ladder then ValidUpdate
            else
                logInfo "[%s, %s] duplicate update" currentSnapshot.region currentSnapshot.bracket.url
                DuplicateUpdate

    let isValid (t: TeamEntry) =
        (not (isNull t.players)) && t.players.Length <> 0

    let syncObj = obj()

    let processUpdate snapshot snapshotHistory teamHistory (storage: Storage) (updatePublisher: TopicClient) (historyStorage: Storage) =
        let currentSnapshotHistory = snapshotHistory |> List.filter isCurrent

        if currentSnapshotHistory |> List.exists (fun entry -> entry.ladder = snapshot.ladder) then
            logInfo "[%s, %s] Snapshot found in history. Skipping..." snapshot.region snapshot.bracket.url
            currentSnapshotHistory, teamHistory
        else
            match currentSnapshotHistory with
            | previousSnapshot :: tail  ->

                match validateUpdate snapshot previousSnapshot with
                | ValidUpdate ->
                    let teams = 
                        findTeams snapshot previousSnapshot
                        |> List.filter (fun t -> t.players.Length = snapshot.bracket.teamSize)

                    for team in teams do 
                        logInfo "<<< [%s, %s] Team found: %A >>>" snapshot.region snapshot.bracket.url team

                    let newTeamHistory = (teams @ teamHistory) |> List.filter isValid

                    if teams.Length <> 0 then
                        logInfo "[%s, %s] Posting ladder update..." snapshot.region snapshot.bracket.url

                        let teamLadder = calculateLadder newTeamHistory
                    
                        let ladderUrl = storage.upload (teamLadder)

                        use msg = new BrokeredMessage {
                            storageLocation = ladderUrl
                            region = snapshot.region
                            bracket = snapshot.bracket
                        }

                        let savedUri = historyStorage.upload(newTeamHistory)
                        logInfo "Current history uploaded to %A" savedUri

                        logInfo "[%s, %s] publishing update message %A" snapshot.region snapshot.bracket.url msg

                        lock syncObj (fun () -> 
                            updatePublisher.Send msg
                        )
                    else
                        logInfo "[%s, %s] No new teams found." snapshot.region snapshot.bracket.url
                    
                    snapshot :: currentSnapshotHistory, newTeamHistory
                | DuplicateUpdate -> 
                    logInfo "[%s, %s] Duplicate update. Skipping..." snapshot.region snapshot.bracket.url
                    currentSnapshotHistory, teamHistory
                | OutdatedUpdate ->
                    logInfo "[%s, %s] Outdated update. Skipping..." snapshot.region snapshot.bracket.url
                    currentSnapshotHistory, teamHistory
                | ExcessiveUpdate ->
                    logInfo "[%s, %s] Excessive update. Skipping..." snapshot.region snapshot.bracket.url
                    snapshot :: currentSnapshotHistory, teamHistory
            | _ -> 
                logInfo "%s, %s Snapshot history is empty, couldn't calculate updates. Added snapshot to history." snapshot.region snapshot.bracket.url
                snapshot :: currentSnapshotHistory, teamHistory