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

    let updateProcessor region bracket storage topic (historyStorage: Storage) (initialHistory: TeamEntry list) = 
        Agent<UpdateProcessorMessage>.Start(fun agent ->

        let processorId = sprintf "[%s, %s]" region bracket.url
        logInfo "UpdateProcessor for %s started with backfill data of %i entries" processorId initialHistory.Length

        Athena.sendUpdate initialHistory region bracket storage historyStorage topic

        let rec loop snapshotHistory teamHistory isFirstUpdate = async {
            let! updateMsg = agent.Receive()

            match updateMsg with
            | UpdateMessage(storageLocation) ->
                let newSnapshotHistory, newTeamHistory = 
                    try
                        logInfo "[%s, %i] Processing update %A..." processorId (teamHistory |> List.length) storageLocation
                        let snapshot = fetch<LadderSnapshot<PlayerEntry>> storageLocation

                        Athena.logAthenaEvent snapshot "processing_update" ""

                        let newSnapshotHistory, newTeamHistory = 
                            Athena.processUpdate snapshot snapshotHistory teamHistory storage topic historyStorage

                        logInfo "[%s, %i] Update %A processed." processorId (newTeamHistory |> List.length) storageLocation
                        newSnapshotHistory, newTeamHistory
                    with
                    | ex -> 
                        FotM.Hephaestus.GoogleAnalytics.sendEvent "UA-49247455-4" "Athena" {
                            category = sprintf "Athena_exception" 
                            action = sprintf "%s_%s" region bracket.url
                            label = (string ex)
                            value = ""
                        } |> ignore
                        logError "Exception while handling message for %s: %A" processorId ex
                        snapshotHistory, teamHistory

                return! loop newSnapshotHistory newTeamHistory false

            | StopMessage ->
                logInfo "UpdateProcessor for %s stopped." processorId
        }

        loop [] initialHistory true
    )

    let getStorage region bracket =
        let prefix = sprintf "%s/%s" region.code bracket.url
        Storage(GlobalSettings.teamLaddersContainer, pathPrefix = prefix)

    let getHistoryStorage region bracket =
        let prefix = sprintf "%s/%s" region.code bracket.url
        Storage(GlobalSettings.athenaHistoryContainer, pathPrefix = prefix)

    let watch (updateListener: SubscriptionClient) (updatePublisher: TopicWrapper) (waitHandle: WaitHandle) =
        logInfo "FotM.Athena entry point called, starting listening to armory updates..."

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
                let processorId = sprintf "[%s, %s]" region.code bracket.url

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

                (region.code, bracket), updateProcessor region.code bracket storage updatePublisher historyStorage backfillData
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
