module Main

open System
open ArmoryLoader
open Armory

let usRegion = {
    region = US;
    blizzardApiUrl = "http://us.battle.net/api/wow/leaderboard/";
    azureConnectionString = ""
}

let region = usRegion

let armoryLoader = ArmoryLoader(region, brackets.threes)

let ladder = armoryLoader.load()

let realms = ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.realm)

[<EntryPoint>]
let main argv = 

    realms
    |> Seq.sortBy (fun g -> snd(g) |> Seq.length )
    |> Seq.iter (fun g -> printfn "%A" g)

    0 // return an integer exit code
