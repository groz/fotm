namespace FotM.Hermes

(*
Workflow of this module:
    - Constantly scan all brackets of all armory regions
    - Store the latest update
    - If that update wasn't seen yet
        - Save it in Azure storage
        - Post update message with storage location to an Azure topic    
*)

open System
open System.Threading
open FotM.Data
open NodaTime
open Microsoft.WindowsAzure.Storage

type Watcher(region: RegionalSettings, bracket: Bracket) =

    let armoryPollTimeout  = Duration.FromSeconds(int64 5)
    let duplicateCheckPeriod = Duration.FromHours(int64 1)

    let armoryLoader = new ArmoryLoader(region, bracket)

    let shouldRetain(snapshot: LadderSnapshot) =
         (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let wait() =
        printfn "Sleeping for %A..." armoryPollTimeout
        Thread.Sleep(armoryPollTimeout.ToTimeSpan())

    let rec armoryUpdates(armoryLoader: ArmoryLoader, oldHistory) = 
        seq {
            let history = oldHistory |> List.filter shouldRetain
            let currentSnapshot = armoryLoader.load()

            if history |> List.exists (fun entry -> entry.ladder = currentSnapshot.ladder) then
                printfn "Duplicate snapshot, skipping..."
                wait()
                yield! armoryUpdates(armoryLoader, history)
            else
                yield currentSnapshot
                wait()
                yield! armoryUpdates(armoryLoader, currentSnapshot :: history)
        }

    let consume snapshot = 
        printfn "Added new snapshot  %A..." (hash(snapshot))

    member this.watch() =
    
        for update in armoryUpdates(ArmoryLoader(region, bracket), []) do
            consume update

module Main =

    (*
    let region = Regions.US

    let armory = ArmoryLoader(region, Brackets.threes)

    let ladderSnapshot = armory.load()
    let realms = ladderSnapshot.ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.classSpec)

    realms
    |> Seq.sortBy (fun g -> snd(g) |> Seq.length )
    |> Seq.iter (fun g -> printfn "%A" g)
    *)

    let watcher = Watcher(Regions.US, Brackets.threes)

    [<EntryPoint>]
    let main argv = 
        printfn "arguments: %A" argv

        watcher.watch()

        0 // return an integer exit code
