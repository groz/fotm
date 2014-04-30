namespace FotM.Hermes

(*
Workflow of this module:
    - Constantly scan all brackets of all armory regions
    - If the current update wasn't seen yet
        - Save it in Azure storage
        - Post update message with storage location to an Azure topic    
*)

open System
open System.Threading
open FSharp.Data
open FotM.Data
open Newtonsoft.Json
open NodaTime.Serialization.JsonNet
open NodaTime

module Main =

    let armoryPollTimeout  = Duration.FromSeconds(20L)
    let duplicateCheckPeriod = Duration.FromHours(1L)

    let shouldRetain(snapshot: LadderSnapshot) =
         (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let wait() =
        printfn "waiting for %A" armoryPollTimeout
        Thread.Sleep(armoryPollTimeout.ToTimeSpan())

    let rec armoryUpdates(region, bracket, oldHistory) = seq {
        let history = oldHistory |> List.filter shouldRetain
        let currentSnapshot = ArmoryLoader.load(region, bracket)

        if history |> List.exists (fun entry -> entry.ladder = currentSnapshot.ladder) then
            wait()
            yield! armoryUpdates(region, bracket, history)
        else
            yield currentSnapshot
            wait()
            yield! armoryUpdates(region, bracket, currentSnapshot :: history)
    }

    type ArmorySettings = {
        repo: SnapshotRepository
        updates: seq<LadderSnapshot>
    }

    let armories = 
        [for region in Regions.all do
            for bracket in Brackets.all do
            yield { 
            repo = SnapshotRepository(region, bracket)
            updates = armoryUpdates(region, bracket, [])
        }];

    let processArmory armory = async {
        try
            for snapshot in armory.updates do
                let uri = armory.repo.uploadSnapshot snapshot
                printfn "uploaded update to %A" uri
        with
            | ex -> printfn "%A" ex
    }

    armories
    |> List.map processArmory
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore