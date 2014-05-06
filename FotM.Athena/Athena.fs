namespace FotM.Athena

open System
open System.Net
open FotM.Data
open FotM.Hephaestus.TraceLogging
open Math
open FotM.Aether
open Microsoft.ServiceBus.Messaging
open System.Threading
open Newtonsoft.Json

type UpdateAgentMessage =
| Update of Uri
| Stop

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
    
    let processUpdate ladderSnapshot history =
        match history with
        | [] -> []
        | head :: tail ->
            let updates = calcUpdates(ladderSnapshot, head)
            let groups = split updates
            let teams = groups |> List.collect findTeams
            teams

    let updateProcessor region bracket = MailboxProcessor<UpdateAgentMessage>.Start(fun agent ->
        logInfo "UpdateProcessor for %s, %s started" region.code bracket.url

        let snapshotRepo = SnapshotRepository(region, bracket)
        let teamRepo = SnapshotRepository(region, bracket)
        
        let rec loop history = async {
            // TODO: filter out expired entries from history
            let! updateMsg = agent.Receive()

            match updateMsg with
            | Update(storageLocation) ->
                try
                    use webClient = new WebClient()
                    logInfo "fetching laddersnapshot from %A" storageLocation
                    let! snapshotJson =  webClient.AsyncDownloadString storageLocation
                    let snapshot = JsonConvert.DeserializeObject<LadderSnapshot> snapshotJson
                    let teams = processUpdate snapshot history
                    return! loop (snapshot::history)
                with
                | ex -> 
                    logError "Exception while handling message for %s, %s: %A" region.code bracket.url ex
                    return! loop history
            | Stop ->
                logInfo "UpdateProcessor for %s, %s stopped." region.code bracket.url
        }

        loop []
    )

    let watch (updateTopic: SubscriptionClient) (waitHandle: WaitHandle) = async {
        logInfo "FotM.Athena entry point called, starting listening to armory updates..."

        // creating processor agents
        let processors = 
            [
                for region in Regions.all do
                for bracket in Brackets.all do
                yield (region, bracket), updateProcessor region bracket
            ]
            |> Map.ofList

        // subscribing to messages
        updateTopic.OnMessage(fun msg->
            logInfo "************ UpdateMessage received: %A ***********" msg
            let body = msg.GetBody<UpdateMesage>()

            let processor = processors.[body.region, body.bracket]
            processor.Post (Update body.storageLocation)
        )

        // waiting until service requests shutdown
        waitHandle.WaitOne() |> ignore

        // stopping processor agents
        for p in processors do p.Value.Post Stop
    }