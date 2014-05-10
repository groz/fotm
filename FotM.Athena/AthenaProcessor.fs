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

type UpdateAgentMessage =
| Update of Uri
| Stop

module AthenaProcessor =
    let updateProcessor region bracket = MailboxProcessor<UpdateAgentMessage>.Start(fun agent ->
        logInfo "UpdateProcessor for %s, %s started" region.code bracket.url

        let snapshotRepo = SnapshotRepository(region, bracket)
        let teamRepo = SnapshotRepository(region, bracket)
        
        let rec loop history = async {
            let! updateMsg = agent.Receive()

            match updateMsg with
            | Update(storageLocation) ->
                try
                    logInfo "Fetching ladder snapshot from %A for %s, %s..." storageLocation region.code bracket.url
                    use webClient = new WebClient()
                    let! snapshotJson =  webClient.AsyncDownloadString storageLocation
                    let snapshot = JsonConvert.DeserializeObject<PlayerLadderSnapshot> snapshotJson
                    logInfo "Processing snapshot for %s, %s..." region.code bracket.url
                    return! loop (Athena.processUpdate snapshot history)
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
        updateTopic.OnMessage(fun msg ->
            logInfo "UpdateMessage received: %A" msg
            let body = msg.GetBody<UpdateMesage>()

            let processor = processors.[body.region, body.bracket]
            processor.Post (Update body.storageLocation)
            msg.Complete()
        )

        // waiting until service requests shutdown
        waitHandle.WaitOne() |> ignore

        // stopping processor agents
        for p in processors do p.Value.Post Stop
    }

