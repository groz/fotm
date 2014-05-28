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

module Helpers =
    let time (blob: CloudBlockBlob) =
        blob.Properties.LastModified.Value.ToUniversalTime().DateTime

open Helpers

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
    member this.time = (time blob).ToString()
    member this.size = blob.Properties.Length

type BlobListViewModel (blobs: CloudBlockBlob array) =
    let firstBlob = blobs.[0]
    let lastBlob = blobs.[blobs.Length-1]

    member this.allBlobs = 
        blobs 
        |> Array.rev
        |> Array.map(fun b -> BlobViewModel b)

    member this.totalSize = blobs |> Array.sumBy(fun b -> b.Properties.Length)
    member this.firstDate = time(firstBlob).ToString()
    member this.lastDate = time(lastBlob).ToString()
    member this.updatesPerMinute = blobs.Length ./ (time(lastBlob) - time(firstBlob)).TotalMinutes
    member this.averageInterval = 
       let intervals = 
        blobs 
        |> Seq.map time 
        |> Seq.windowed 2 
        |> Seq.map(fun arr -> arr.[1] - arr.[0]) 
       let avgInMinutes = intervals |> Seq.averageBy(fun x -> x.TotalMinutes)
       TimeSpan.FromMinutes avgInMinutes

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

    let seen teamInfo = teamInfo.lastEntry.snapshotTime

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

        let snapshotTime = filteredTeams |> Seq.maxBy(fun t -> seen (snd t))

        teamsToShow, setupsToShow, armoryInfo.snapshotUrl

    [<Route("{region}/{bracket}/now")>]
    member this.Get(region: string, bracket: string) =
        let now = NodaTime.SystemClock.Instance.Now

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
        let blobs = s.allBlobs(prefix)        
        BlobListViewModel blobs

    [<HttpGet>]
    [<Route("showBlob")>]
    member this.ShowBlob([<FromUri>]blobUri: string) =
        let json = StorageIO.download (Uri blobUri)

        let response = this.Request.CreateResponse(HttpStatusCode.OK)

        response.Content <- new StringContent(json, Encoding.UTF8, "application/json")

        response