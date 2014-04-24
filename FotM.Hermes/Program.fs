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
open FotM.Data

module Main =

    let region = Regions.US

    let armory = ArmoryLoader(region, Brackets.threes)

    let ladderSnapshot = armory.load()

    let realms = ladderSnapshot.ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.classSpec)

    realms
    |> Seq.sortBy (fun g -> snd(g) |> Seq.length )
    |> Seq.iter (fun g -> printfn "%A" g)


    [<EntryPoint>]
    let main argv = 
        printfn "%A" argv
        0 // return an integer exit code
