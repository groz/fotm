namespace FotM.Hermes

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
