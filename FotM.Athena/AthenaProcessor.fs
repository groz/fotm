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

    let updateProcessor processorId storage topic (historyStorage: Storage) (initialHistory: TeamEntry list) = 
        Agent<UpdateProcessorMessage>.Start(fun agent ->

        logInfo "UpdateProcessor for %s started with backfill data of %i entries" processorId initialHistory.Length

        let rec loop (snapshotHistory, teamHistory) = async {
            let! updateMsg = agent.Receive()

            match updateMsg with
            | UpdateMessage(storageLocation) ->
                let newSnapshotHistory, newTeamHistory = 
                    try
                        logInfo "[%s, %i] Processing update %A..." processorId (teamHistory |> List.length) storageLocation
                        let snapshot = fetch<LadderSnapshot<PlayerEntry>> storageLocation
                        let newSnapshotHistory, newTeamHistory = 
                            Athena.processUpdate snapshot snapshotHistory teamHistory storage topic historyStorage
                        logInfo "[%s, %i] Update %A processed." processorId (newTeamHistory |> List.length) storageLocation
                        newSnapshotHistory, newTeamHistory
                    with
                    | ex -> 
                        logError "Exception while handling message for %s: %A" processorId ex
                        snapshotHistory, teamHistory
                return! loop (newSnapshotHistory, newTeamHistory)
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

    let watch (updateListener: SubscriptionClient) (updatePublisher: TopicWrapper) (waitHandle: WaitHandle) =
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
                let historyStorage = getHistoryStorage region bracket

                let allBlobs = historyStorage.allFiles (region.code + "/" + bracket.url)

                let last = allBlobs |> Array.rev |> Seq.tryFind(fun _ -> true)

                let backfillData = 
                    match last with
                    | Some blobUri -> 
                        logInfo "History data for %s found at %A, loading..." processorId blobUri
                        let data: TeamEntry list = fetch<TeamEntry list> blobUri
                        logInfo "%s total history entries: %i" processorId data.Length
                        data
                    | None -> 
                        logInfo "History data for %s not found." processorId
                        []

                let storage = getStorage region bracket

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
