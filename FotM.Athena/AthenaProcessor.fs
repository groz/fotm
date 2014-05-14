namespace FotM.Athena

open System
open Math
open FotM.Data
open FotM.Aether
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

    let fetchSnapshot storageLocation = async {
        logInfo "Fetching ladder snapshot from %A" storageLocation
        use webClient = new WebClient()
        let! snapshotJson =  webClient.AsyncDownloadString storageLocation
        return JsonConvert.DeserializeObject<PlayerLadderSnapshot> snapshotJson
    }

    let updateProcessor processorId storage topic = Agent<UpdateProcessorMessage>.Start(fun agent ->
        logInfo "UpdateProcessor for %s started" processorId

        let rec loop (snapshotHistory, teamHistory) = async {
            let! updateMsg = agent.Receive()

            match updateMsg with
            | UpdateMessage(storageLocation) ->
                try
                    let! snapshot = fetchSnapshot storageLocation
                    return! loop (Athena.processUpdate snapshot snapshotHistory teamHistory storage topic)
                with
                | ex -> 
                    logError "Exception while handling message for %s: %A" processorId ex
                    return! loop (snapshotHistory, teamHistory)
            | StopMessage ->
                logInfo "UpdateProcessor for %s stopped." processorId
        }

        loop ([], [])
    )

    let getStorage region bracket =
        let prefix = sprintf "%s/%s" region.code bracket.url
        Storage("ladders", pathPrefix = prefix)

    let watch (updateListener: SubscriptionClient) (updatePublisher) (waitHandle: WaitHandle) =
        logInfo "FotM.Athena entry point called, starting listening to armory updates..."

        let getProcessorId region bracket = sprintf "[%s, %s]" region.code bracket.url

        // creating processor agents
        let processors = 
            [
                for region in Regions.all do
                for bracket in Brackets.all do
                yield (region.code, bracket), updateProcessor (getProcessorId region bracket) (getStorage region bracket) updatePublisher
            ]
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
