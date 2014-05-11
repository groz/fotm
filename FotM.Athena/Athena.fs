namespace FotM.Athena

open System
open FotM.Data
open FotM.Hephaestus.TraceLogging
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
        |> Array.map (fun currentEntry -> currentEntry, previousMap.TryFind(currentEntry.player))
        |> Array.map (
            function
            | current, None -> None
            | current, Some(previous) when current = previous -> None
            | current, Some(previous) -> Some(current, previous))
        |> Array.choose id
        |> Array.map (fun (current, previous) -> current - previous)
        |> Array.toList

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
                let subPartitions = // TODO: fix this non-sense
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

    let findTeamsInGroup(updateGroup: PlayerUpdate list): PlayerUpdate list list =
        // TODO: implement this
        let clusterer = AthenaKMeans(featureExtractor, true, true)
        let clusters = clusterer.computeGroups (updateGroup |> List.toArray) 3
        []
    
    let findTeams snapshot snapshotHistory teamHistory =
        match snapshotHistory with
        | [] -> []
        | previousSnapshot :: tail ->
            let updates = calcUpdates snapshot previousSnapshot
            let groups = split updates
            let teams = groups |> List.collect findTeamsInGroup
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
