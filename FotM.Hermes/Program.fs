﻿namespace FotM.Hermes

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

    let armoryPollTimeout  = Duration.FromSeconds(int64 20)
    let duplicateCheckPeriod = Duration.FromHours(int64 1)

    let shouldRetain(snapshot: LadderSnapshot) =
         (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let wait() =
        printfn "Sleeping for %A..." armoryPollTimeout
        Thread.Sleep(armoryPollTimeout.ToTimeSpan())

    let rec armoryUpdates(region, bracket, oldHistory) = seq {
        let history = oldHistory |> List.filter shouldRetain
        let currentSnapshot = ArmoryLoader.load(region, bracket)

        if history |> List.exists (fun entry -> entry.ladder = currentSnapshot.ladder) then
            printfn "Duplicate snapshot, skipping..."
            wait()
            yield! armoryUpdates(region, bracket, history)
        else
            yield currentSnapshot
            wait()
            yield! armoryUpdates(region, bracket, currentSnapshot :: history)
    }

    type ArmoryStream = {
        uploader: MailboxProcessor<UploadRequestMessage>
        updates: seq<LadderSnapshot>
    }

    [<EntryPoint>]
    let main argv = 
        printfn "arguments: %A" argv

        let armories = 
            [for region in Regions.all do
             for bracket in Brackets.all do
             yield { 
                uploader = SnapshotRepository(region, bracket).uploader
                updates = armoryUpdates(region, bracket, [])
            }];

        let buildMessage(replyChannel): AsyncReplyChannel<Uri> -> LadderSnapshot = 
            replyChannel()

        let processArmory armory = async {
            for snapshot in armory.updates do
                let result = armory.uploader.PostAndReply(fun replyChannel -> snapshot, replyChannel)
                printfn "Added new snapshot %A..." result

                // TODO: publish update
        }

        armories
        |> List.map processArmory
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        0 // return an integer exit code