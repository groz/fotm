﻿namespace FotM.Apollo

open System
open System.Collections.Generic
open System.Web.Configuration
open System.Net
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.Async
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.Math
open FotM.Hephaestus.CollectionExtensions
open Newtonsoft.Json
open FotM.Aether.StorageIO

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
                let updatedArmories =
                    try
                        let snapshot = fetch<TeamInfo list> storageLocation
                        let armoryInfo = ArmoryInfo(snapshot, storageLocation)
                        let updatedArmories = armories |> Map.add(region, bracket) armoryInfo

                        let lastTime = snapshot |> List.map (fun e -> e.lastEntry.snapshotTime) |> List.max

                        repository.update updatedArmories
                        PlayingNowUpdateManager.notifyUpdateReady(region, bracket, lastTime)
                        updatedArmories
                    with
                    | ex -> 
                        logError "Exception while handling message for %s, %s: %A" region bracket ex
                        armories
                return! loop updatedArmories
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
            let allBlobs = storage.allBlobs (dir region bracket)

            let last = 
                allBlobs 
                |> Array.rev 
                |> Seq.map(fun blob -> 
                    try
                        fetch<TeamInfo list> blob.Uri, blob.Uri
                    with
                    | ex -> 
                        blob.Delete()
                        [], Uri("")
                    )
                |> Seq.skipWhile(fun (teams, uri) -> teams.IsEmpty)
                |> Seq.tryFind(fun _ -> true)

            match last with
            | Some (teams, uri) -> 
                logInfo "Backfilling for %s, %s from %A" region bracket uri
                Some((region, bracket), ArmoryInfo(teams, uri))
            | None -> 
                logInfo "No data found for backfill of %s, %s" region bracket
                None)
        |> Map.ofSeq

    let storageConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.Storage.ConnectionString"]
    let serviceBusConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.ServiceBus.ConnectionString"]

    let OnStart(): unit = 
            
        let storage = Storage(GlobalSettings.teamLaddersContainer, storageConnectionString.ConnectionString)

        let backfillData = backfillFrom storage
        
        let armoryAgent = createArmoryAgent backfillData repository

        let serviceBus = ServiceBus(serviceBusConnectionString.ConnectionString)

        // TODO: fix for local
        let subscriptionName = Dns.GetHostName() + Guid.NewGuid().ToString().Substring(0, 4)

        let updateTopic = serviceBus.subscribe GlobalSettings.teamUpdatesTopic subscriptionName

        updateTopic.OnMessage(fun brokeredMessage ->
            let msg = brokeredMessage.GetBody<UpdateMesage>()

            let updateArmoryMessage = UpdateArmory(msg.region, msg.bracket.url, msg.storageLocation)
            armoryAgent.Post updateArmoryMessage

            brokeredMessage.Complete()
        )

