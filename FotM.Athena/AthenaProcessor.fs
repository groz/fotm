namespace FotM.Athena

open System
open FotM.Hephaestus.Math
open FotM.Data
open FotM.Aether
open FotM.Aether.StorageIO
open FotM.Hephaestus.TraceLogging
open Microsoft.ServiceBus.Messaging
open System.Threading
open Newtonsoft.Json
open NodaTime
open System.Net
open FotM.Hephaestus.Async

type UpdateProcessorMessage =
| UpdateMessage of Uri
| StopMessage

module AthenaProcessor =

    let updateProcessor processorId storage topic (historyStorage: Storage) initialHistory = Agent<UpdateProcessorMessage>.Start(fun agent ->
        logInfo "UpdateProcessor for %s started" processorId

        let rec loop (snapshotHistory, teamHistory) = async {
            let! updateMsg = agent.Receive()

            match updateMsg with
            | UpdateMessage(storageLocation) ->
                try
                    logInfo "Processing update %A" storageLocation
                    let! snapshot = fetch storageLocation
                    let newSnapshotHistory, newTeamHistory = Athena.processUpdate snapshot snapshotHistory teamHistory storage topic historyStorage
                    return! loop (newSnapshotHistory, newTeamHistory)
                with
                | ex -> 
                    logError "Exception while handling message for %s: %A" processorId ex
                    return! loop (snapshotHistory, teamHistory)
            | StopMessage ->
                logInfo "UpdateProcessor for %s stopped." processorId
        }

        loop ([], initialHistory)
    )

    let getStorage region bracket =
        let prefix = sprintf "%s/%s" region.code bracket.url
        Storage(GlobalSettings.teamLaddersContainer, pathPrefix = prefix)

    let getHistoryStorage region bracket =
        let prefix = sprintf "%s/%s" region.code bracket.url
        Storage(GlobalSettings.athenaHistoryContainer, pathPrefix = prefix)

    let watch (updateListener: SubscriptionClient) (updatePublisher) (waitHandle: WaitHandle) =
        logInfo "FotM.Athena entry point called, starting listening to armory updates..."

        let getProcessorId region bracket = sprintf "[%s, %s]" region.code bracket.url

        let historyStorage = Storage GlobalSettings.athenaHistoryContainer

        // creating processor agents
        let allRoots = 
            [
                for region in Regions.all do
                for bracket in Brackets.all do
                yield (region, bracket)
            ]

        // init with backfill
        let processors = 
            allRoots
            |> List.map(fun (region, bracket) -> 
                let processorId = getProcessorId region bracket
                let storage = getStorage region bracket
                let historyStorage = getHistoryStorage region bracket

                let allBlobs = storage.allFiles (region.code + "/" + bracket.url)

                let last = allBlobs |> Array.rev |> Seq.tryFind(fun _ -> true)

                let backfillData = 
                    match last with
                    | Some blobUri -> 
                        logInfo "History data for %s found at %A, loading..." processorId blobUri
                        let data = fetch blobUri |> Async.RunSynchronously
                        data
                    | None -> 
                        logInfo "History data for %s not found." processorId
                        []

                (region.code, bracket), updateProcessor processorId storage updatePublisher historyStorage backfillData
            )            
            |> Map.ofList

        // subscribing to messages
        updateListener.OnMessage(fun msg ->
            logInfo "UpdateMessage received: %A" msg
            let body = msg.GetBody<UpdateMesage>()

            let processor = processors.[body.region, body.bracket]
            processor.Post (UpdateMessage body.storageLocation)
            msg.Complete()
        )

        // waiting until service requests shutdown
        waitHandle.WaitOne() |> ignore

        // stopping processor agents
        for p in processors do 
            p.Value.Post StopMessage
