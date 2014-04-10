module Main

open System
open Loading
open Armory

let usRegion = {
    region = US;
    blizzardApiUrl = "http://us.battle.net/api/wow/leaderboard/";
    azureConnectionString = ""
}

let region = usRegion

let armoryLoader = ArmoryLoader(region, brackets.threes)

let ladder = armoryLoader.load()

open Armory

let realms = ladder |> Seq.groupBy (fun (playerEntry: PlayerEntry) -> playerEntry.player.realm)

realms
|> Seq.sortBy (fun g -> snd(g) |> Seq.length )
|> Seq.iter (fun g -> printfn "%A" g)
