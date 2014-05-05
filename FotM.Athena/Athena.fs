namespace FotM.Athena

open FotM.Data
open FotM.Hephaestus.TraceLogging
open Math

module Athena =
    (*
        1. calculate updates/diffs
        2. split into non-intersecting groups
        3. find best k-partition for each group, i.e. teams for each group
        4. merge teams
        5. post update
    *)

    let calcUpdates (currentSnapshot: LadderSnapshot, previousSnapshot: LadderSnapshot) =
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
                let subPartitions = 
                    currentPartitions
                    |> List.map (List.partition headCondition)      // partitions into tuples of (list, list)
                    |> List.map (fun (left, right) -> left @ right) // merge tuple into list

                partitionAll(subPartitions, tail)
        
        partitionAll([updates], splitConditions)

    let findTeams(updateGroup: PlayerUpdate list): PlayerUpdate list list =
        // TODO: implement this
        []
    
    let processUpdate(ladderSnapshot: LadderSnapshot, history: LadderSnapshot list) =
        match history with
        | [] -> []
        | head :: tail ->
            let updates = calcUpdates(ladderSnapshot, head)
            let groups = split updates
            let teams = groups |> List.collect findTeams
            teams

    let watch = async {
        logInfo "FotM.Athena entry point called, starting listening to armory updates..."
        return ()
    }
