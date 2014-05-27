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
open Microsoft.WindowsAzure.Storage.Blob

type TeamViewModel (rank: int, teamInfo: TeamInfo, justPlayed: bool)=
    member this.rank = rank
    member this.factionId = int (teamInfo.lastEntry.players |> Seq.head).faction
    member this.rating = teamInfo.lastEntry.rating
    member this.ratingChange = teamInfo.lastEntry.ratingChange
    member this.seen = teamInfo.lastEntry.snapshotTime.ToDateTimeUtc().ToString()
    member this.players = 
        teamInfo.lastEntry.players
        |> List.sortBy(fun p -> p.classSpec.isHealer, p.classSpec.isRanged, Specs.getClassId p.classSpec, p.name, p.realm)
    member this.justPlayed = justPlayed
    member this.wins = teamInfo.totalWins
    member this.losses = teamInfo.totalLosses

type SetupViewModel (rank: int, specs: Class list, ratio: float) =
    member this.rank = rank
    member this.specs = specs
    member this.percent = sprintf "%.1f%%" (ratio * 100.0)

type BlobViewModel (blob: CloudBlockBlob) =
    member this.uri = blob.Uri
    member this.time = blob.Properties.LastModified.Value.ToUniversalTime().DateTime.ToString()
    member this.size = blob.Properties.Length

/// Retrieves values.
[<RoutePrefix("api")>]
type ValuesController() =
    inherit ApiController()

    let maxLeaderboardTeams = 15
    let maxSpecs = 10
    let maxPlayingNow = 50

    let playingNowPeriod = NodaTime.Duration.FromHours(1L)
    let justPlayedPeriod = NodaTime.Duration.FromMinutes(20L)

    let parseFilters (filters: string seq) =
        filters
        |> Seq.map (fun str -> 
            let dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(str)
            Specs.fromString (dict.["className"]) (dict.["specId"]) )
        |> Seq.choose id
        |> Seq.toArray

    [<Route("{region}/{bracket}")>]
    member this.Get(region: string, bracket: string, [<FromUri>]filters: string seq) =
        let fotmFilters = parseFilters filters

        let armoryInfo = Main.repository.getArmory(region, bracket)

        let filteredTeams =
            armoryInfo.teams
            |> Seq.filter (fun (i, t) -> t |> Teams.teamMatchesFilter fotmFilters)

        let filteredSetups = 
            armoryInfo.setups
            |> Seq.mapi(fun i setup -> i+1, setup)
            |> Seq.filter(fun (rank, setup) -> fst setup |> Teams.matchesFilter fotmFilters)
            
        let teamsToShow = 
            filteredTeams 
            |> Seq.map(fun (rank, team) -> TeamViewModel(rank, team, false))
            |> Seq.truncate maxLeaderboardTeams

        let setupsToShow = 
            filteredSetups 
            |> Seq.map(fun (rank, s) -> SetupViewModel(rank, fst s, snd s ./. armoryInfo.totalGames))
            |> Seq.truncate maxSpecs

        teamsToShow, setupsToShow

    [<Route("{region}/{bracket}/now")>]
    member this.Get(region: string, bracket: string) =
        let now = NodaTime.SystemClock.Instance.Now

        let seen teamInfo = teamInfo.lastEntry.snapshotTime

        let armoryInfo = Main.repository.getArmory(region, bracket)

        let filteredTeams =
            armoryInfo.teams
            |> Seq.filter(fun (rank, team) -> now - seen team < playingNowPeriod)
            |> Seq.map(fun (rank, team) -> rank, team, now - seen team < justPlayedPeriod)
        
        filteredTeams 
        |> Seq.map(fun t -> TeamViewModel t)
        |> Seq.truncate maxPlayingNow
            
    [<Route("listBlobs")>]
    member this.GetListBlobs([<FromUri>]container: string, [<FromUri>]prefix: string) =
        let s = Storage(container, Main.storageConnectionString.ConnectionString)
        s.allBlobs(prefix)
        |> Array.map(fun b -> BlobViewModel b)