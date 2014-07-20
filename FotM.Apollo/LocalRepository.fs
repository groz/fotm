namespace FotM.Apollo

open System
open FotM.Data
open FotM.Hephaestus.Math
open FotM.Hephaestus.CollectionExtensions

type ArmoryAgentMessage =
| UpdateArmory of region: string * bracket : string * storageLocation: Uri
| StopAgent

type ArmoryInfo(ladder : TeamInfo list, storageLocation: Uri) =

    member this.snapshotUrl = storageLocation

    member this.teams = 
        ladder
        |> Seq.filter(fun t -> not (isNull t.lastEntry.players))
        |> Seq.filter(fun t -> t.totalGames >= 3)
        |> Seq.sortBy(fun t -> -t.lastEntry.rating) 
        |> Seq.mapi (fun i t -> i+1, t)
        |> Seq.toArray

    member this.totalGames = this.teams |> Seq.sumBy(fun (rank, team) -> team.totalGames)

    member this.setups =
        this.teams
        |> Seq.groupBy (fun (rank, teamInfo) -> teamInfo.lastEntry.getClasses())
        |> Seq.map (fun (specs, group) -> 
            let totalGames = group |> Seq.sumBy(fun (rank, teamInfo) -> teamInfo.totalGames)
            let totalWins = group |> Seq.sumBy(fun (rank, teamInfo) -> teamInfo.totalWins)
            specs, totalGames, totalWins ./. totalGames)

type Repository() =
    let mutable armoryData = Map.empty<string*string, ArmoryInfo>

    member this.update newData = armoryData <- newData // accessed from a threadsafe agent

    member this.getArmory(region: string, bracket: string) =
        let data = armoryData
        data.TryFind(region.ToUpper(), bracket)
