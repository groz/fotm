namespace FotM.Athena

open System
open FotM.Data
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.CollectionExtensions
open Math
open NodaTime

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
                let currentTotal = current.seasonWins + current.seasonLosses
                let previousTotal = previous.seasonWins + previous.seasonLosses

                if currentTotal = previousTotal then
                    None
                else
                    logInfo "%A updated to %A" current previous
                    Some(current - previous)
            )
        |> Seq.choose id
        |> Seq.toList

    let split(updates: PlayerUpdate list): PlayerUpdate list list =
        // 2. split into non-intersecting groups
        let splitConditions = [
            fun (update: PlayerUpdate) -> update.player.faction = Faction.Horde
            fun (update: PlayerUpdate) -> update.ratingDiff > 0
        ]

        let rec partitionAll (currentPartitions: PlayerUpdate list list, splitConditions: (PlayerUpdate -> bool) list): PlayerUpdate list list =
            match splitConditions with
            | [] -> currentPartitions
            | headCondition :: tail -> 
                let subPartitions =
                    currentPartitions
                    |> List.map (List.partition headCondition)      // partitions into tuples of (list, list)
                    |> List.map (fun (left, right) -> left @ right) // merge tuple into list

                partitionAll(subPartitions, tail)
        
        partitionAll([updates], splitConditions)

    let featureExtractor (pu: PlayerUpdate) =
        [|
            float pu.ratingDiff
            float pu.ranking
            float pu.rating
            float pu.weeklyWins
            float pu.weeklyLosses
            float pu.seasonWins
            float pu.seasonLosses
        |]        

    let findTeamsInGroup (teamSize) (updateGroup: PlayerUpdate list) : PlayerUpdate list list =
        let g = updateGroup |> Array.ofList

        if g.Length < teamSize then
            []
        else
            let clusterer = AthenaKMeans(featureExtractor, true, true)
            let clustering = clusterer.computeGroups g teamSize
            
            clustering
            |> Seq.mapi (fun i ci -> ci, g.[i])
            |> toMultiMap
            |> Map.toList
            |> List.map snd

    let findTeams snapshot snapshotHistory teamHistory =
        match snapshotHistory with
        | [] -> []
        | previousSnapshot :: tail ->
            let updates = calcUpdates snapshot previousSnapshot

            logInfo "Total players updated: %i" updates.Length

            let groups = split updates
            let teams = groups |> List.collect (findTeamsInGroup snapshot.bracket.teamSize)
            teams

    let isCurrent snapshot =
        (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let update teamLadder teams =
        teamLadder

    let processUpdate snapshot snapshotHistory teamHistory =
        let currentSnapshotHistory = snapshotHistory |> List.filter isCurrent

        if currentSnapshotHistory |> List.exists (fun entry -> entry.ladder = snapshot.ladder) then
            currentSnapshotHistory, teamHistory
        else
            let teams = findTeams snapshot currentSnapshotHistory teamHistory

            for team in teams do 
                logInfo "%A" team

            let teamLadder = update teamHistory teams

            // TODO: post update

            snapshot :: currentSnapshotHistory, teamLadder
