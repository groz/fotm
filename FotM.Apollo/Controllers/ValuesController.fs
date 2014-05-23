namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open System.Net
open System.Text
open Newtonsoft.Json
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.Math
open FotM.Data
open FotM.Apollo
open FotM.Aether

type TeamViewModel (rank: int, teamInfo: TeamInfo)=
    member this.rank = rank
    member this.players = teamInfo.lastEntry.players
    member this.factionId = int (teamInfo.lastEntry.players |> Seq.head).faction
    member this.rating = teamInfo.lastEntry.rating
    member this.ratingChange = teamInfo.lastEntry.ratingChange
    member this.seen = teamInfo.lastEntry.snapshotTime.ToDateTimeUtc().ToString()

type SetupViewModel (rank: int, specs: Class list, ratio: float) =
    member this.rank = rank
    member this.specs = specs
    member this.percent = sprintf "%.1f%%" (ratio * 100.0)

/// Retrieves values.
[<RoutePrefix("api")>]
type ValuesController() =
    inherit ApiController()

    let url = @"http://127.0.0.1:10000/devstoreaccount1/ladders/US/3v3/883499d2-470e-4d64-8761-93858f7204ad"

    let fetchSnapshot storageLocation =
        let ladderJson = StorageIO.download storageLocation
        JsonConvert.DeserializeObject<TeamInfo list> ladderJson

    let ladder = fetchSnapshot (Uri url)
    let teams = ladder |> Seq.mapi (fun i t -> i+1, t)

    [<Route("{region}/{bracket}")>]
    member this.Get(region: string, bracket: string, [<FromUri>]filters: string seq) =
        let fotmFilters = 
            filters
            |> Seq.map (fun str -> JsonConvert.DeserializeObject<Dictionary<string, string>>(str))
            |> Seq.map (fun dict -> dict.["className"], dict.["specId"])
            |> Seq.map (fun (className, specIdStr) -> 
                let specId = if specIdStr = null then -1 else (int specIdStr)
                Specs.fromString className specId)
            |> Seq.choose id
            |> Seq.toArray

        let filteredTeams =
            teams
            |> Seq.filter (fun (i, t) -> t |> Teams.teamMatchesFilter fotmFilters)

        let totalGames = teams |> Seq.sumBy(fun (rank, team) -> team.totalGames)

        let setups =
            teams
            |> Seq.groupBy (fun (rank, teamInfo) -> teamInfo.lastEntry.getClasses())
            |> Seq.map (fun (specs, group) -> specs, group |> Seq.sumBy(fun (rank, teamInfo) -> teamInfo.totalGames))
            |> Seq.sortBy (fun (specs, count) -> -count)

        let filteredSetups = 
            setups
            |> Seq.mapi(fun i setup -> i+1, setup)
            |> Seq.filter(fun (rank, setup) -> fst setup |> Teams.matchesFilter fotmFilters)

        filteredTeams |> Seq.map(fun t -> TeamViewModel t), 
        filteredSetups |> Seq.map(fun (rank, s) -> SetupViewModel(rank, fst s, snd s ./. totalGames))

    [<Route("{region}/{bracket}/now")>]
    member this.Get(region: string, bracket: string) =
        let now = NodaTime.SystemClock.Instance.Now

        let period = NodaTime.Duration.FromStandardDays(10L)

        let seen teamInfo = teamInfo.lastEntry.snapshotTime

        let filteredTeams =
            teams
            |> Seq.filter(fun (rank, team) -> now - seen team < period)
        
        filteredTeams |> Seq.map(fun t -> TeamViewModel t)