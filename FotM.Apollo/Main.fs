namespace FotM.Apollo

open System
open System.Collections.Generic
open System.Web.Configuration
open System.Net
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.Async
open FotM.Hephaestus.TraceLogging
open Newtonsoft.Json

type ArmoryAgentMessage =
| UpdateArmory of region: string * bracket : string * storageLocation: Uri
| StopAgent

type ArmoryInfo(ladder : TeamInfo list) =
    member this.teams = ladder |> Seq.mapi (fun i t -> i+1, t)
    member this.totalGames = this.teams |> Seq.sumBy(fun (rank, team) -> team.totalGames)
    member this.setups =
        this.teams
        |> Seq.groupBy (fun (rank, teamInfo) -> teamInfo.lastEntry.getClasses())
        |> Seq.map (fun (specs, group) -> specs, group |> Seq.sumBy(fun (rank, teamInfo) -> teamInfo.totalGames))
        |> Seq.sortBy (fun (specs, count) -> -count)

type Repository() =
    let mutable armoryData = 
        [
            for region in Regions.all do
            for bracket in Brackets.all do
            yield (region.code, bracket.url), ArmoryInfo []
        ]
        |> Map.ofSeq

    member this.update(newData) = armoryData <- newData
    member this.data() = armoryData
    member this.getArmory(region: string, bracket: string) =
        let data = this.data()
        data.[region.ToUpper(), bracket]

module Main =

    let fetchSnapshot storageLocation = async {
        let! snapshotJson = StorageIO.downloadAsync storageLocation
        return JsonConvert.DeserializeObject<TeamInfo list> snapshotJson
    }

    let repository = Repository()

    let armoryAgent = Agent<ArmoryAgentMessage>.Start(fun agent ->
        logInfo "ArmoryAgent started..."

        let rec loop (armories: Map<string*string, ArmoryInfo>) = async {
            let! msg = agent.Receive()

            match msg with
            | UpdateArmory(region, bracket, storageLocation) ->
                try
                    let! snapshot = fetchSnapshot storageLocation
                    let armoryInfo = ArmoryInfo snapshot
                    let updatedArmories = armories |> Map.add(region, bracket) armoryInfo
                    repository.update updatedArmories
                    return! loop updatedArmories
                with
                | ex -> 
                    logError "Exception while handling message for %s, %s: %A" region bracket ex
                    return! loop armories
            | StopAgent -> 
                logInfo "ArmoryAgent stopped."
        }

        loop (repository.data())
    )

    let OnStart(): unit = 

        let storageConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.Storage.ConnectionString"]
        let serviceBusConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.ServiceBus.ConnectionString"]
        
        let storage = Storage(GlobalSettings.teamLaddersContainer, storageConnectionString.ConnectionString)
        let serviceBus = ServiceBus(serviceBusConnectionString.ConnectionString)

        let subscriptionName = Dns.GetHostName()

        let updateTopic = serviceBus.subscribe GlobalSettings.teamUpdatesTopic subscriptionName

        updateTopic.OnMessage(fun brokeredMessage ->
            let msg = brokeredMessage.GetBody<UpdateMesage>()

            let updateArmoryMessage = UpdateArmory(msg.region, msg.bracket.url, msg.storageLocation)
            armoryAgent.Post updateArmoryMessage

            brokeredMessage.Complete()
        )