namespace FotM.Hermes

open System

module Main =
    let usRegion = {
        region = US;
        blizzardApiUrl = "http://us.battle.net/api/wow/leaderboard/";
        azureConnectionString = ""
    }

    let region = usRegion

    let armoryPuller = ArmoryPuller(region, brackets.threes)

    let ladderSnapshot = armoryPuller.load()

    let realms = ladderSnapshot.ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.classSpec)

    realms
    |> Seq.sortBy (fun g -> snd(g) |> Seq.length )
    |> Seq.iter (fun g -> printfn "%A" g)


    [<EntryPoint>]
    let main argv = 
        printfn "%A" argv
        0 // return an integer exit code
