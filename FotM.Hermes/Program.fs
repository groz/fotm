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
open NodaTime
open FotM.Data

module Main =

    let armoryPollTimeout  = Duration.FromSeconds(int64 5)
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

    [<EntryPoint>]
    let main argv = 
        printfn "arguments: %A" argv

        let region = Regions.US
        let bracket = Brackets.threes;
        let repo = SnapshotRepository(region, bracket)

        for snapshot in armoryUpdates(region, bracket, []) do

            let snapshotId = repo.uploadSnapshot snapshot
            printfn "Added new snapshot  %A..." snapshotId

        0 // return an integer exit code


(*
let region = Regions.US

let armory = ArmoryLoader(region, Brackets.threes)

let ladderSnapshot = armory.load()
let realms = ladderSnapshot.ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.classSpec)

realms
|> Seq.sortBy (fun g -> snd(g) |> Seq.length )
|> Seq.iter (fun g -> printfn "%A" g)
*)

