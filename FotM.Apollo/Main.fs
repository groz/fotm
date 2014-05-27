namespace FotM.Apollo

open System
open System.Collections.Generic
open System.Web.Configuration
open System.Net
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.Async
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.CollectionExtensions
open Newtonsoft.Json
open FotM.Aether.StorageIO

type ArmoryAgentMessage =
| UpdateArmory of region: string * bracket : string * storageLocation: Uri
| StopAgent

type ArmoryInfo(ladder : TeamInfo list) =

    member this.teams = 
        ladder
        |> Seq.filter(fun t -> not (isNull t.lastEntry.players))
        |> Seq.filter(fun t -> t.totalGames > 3)
        |> Seq.sortBy(fun t -> -t.lastEntry.rating) 
        |> Seq.mapi (fun i t -> i+1, t)
        |> Seq.toArray

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

    let createArmoryAgent (initialData: Map<string*string, ArmoryInfo>) (repository: Repository) = Agent<ArmoryAgentMessage>.Start(fun agent ->

        let show =
            let divisions = 
                initialData
                |> Seq.map(fun kv -> sprintf "%A: %A" kv.Key kv.Value.teams.Length)
            System.String.Join(";", divisions)
                
        logInfo "ArmoryAgent started with initialData: %s total teams..." show
        repository.update initialData

        let rec loop (armories: Map<string*string, ArmoryInfo>) = async {
            let! msg = agent.Receive()

            match msg with
            | UpdateArmory(region, bracket, storageLocation) ->
                try
                    let! snapshot = fetch storageLocation
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

        loop initialData
    )

    let repository = Repository()

    let backfillFrom(storage: Storage) =
        let storageRoots = 
            [
                for region in Regions.all do
                for bracket in Brackets.all do
                yield region.code, bracket.url
            ]

        let dir r b = r + "/" + b

        storageRoots
        |> Seq.choose(fun (region, bracket) -> 
            let allBlobs = storage.allFiles (dir region bracket)

            let last = 
                allBlobs 
                |> Array.rev 
                |> Seq.map(fun blobUri -> fetch<TeamInfo list> blobUri |> Async.RunSynchronously)
                |> Seq.skipWhile(fun teams -> teams.IsEmpty)
                |> Seq.tryFind(fun _ -> true)

            match last with
            | Some teams -> 
                logInfo "Backfilling for %s, %s with %A" region bracket teams
                Some((region, bracket), ArmoryInfo teams)
            | None -> 
                logInfo "No data found for backfill of %s, %s" region bracket
                None)
        |> Map.ofSeq

    let OnStart(): unit = 

        let storageConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.Storage.ConnectionString"]
        let serviceBusConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.ServiceBus.ConnectionString"]
        
        let storage = Storage(GlobalSettings.teamLaddersContainer, storageConnectionString.ConnectionString)

        let backfillData = backfillFrom storage

        let armoryAgent = createArmoryAgent backfillData repository

        let serviceBus = ServiceBus(serviceBusConnectionString.ConnectionString)

        let subscriptionName = Dns.GetHostName()

        let updateTopic = serviceBus.subscribe GlobalSettings.teamUpdatesTopic subscriptionName

        updateTopic.OnMessage(fun brokeredMessage ->
            let msg = brokeredMessage.GetBody<UpdateMesage>()

            let updateArmoryMessage = UpdateArmory(msg.region, msg.bracket.url, msg.storageLocation)
            armoryAgent.Post updateArmoryMessage

            brokeredMessage.Complete()
        )