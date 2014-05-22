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
open FotM.Data
open FotM.Apollo
open FotM.Aether

/// Retrieves values.
[<RoutePrefix("api")>]
type ValuesController() =
    inherit ApiController()

    let url = @"http://127.0.0.1:10000/devstoreaccount1/ladders/US/3v3/883499d2-470e-4d64-8761-93858f7204ad"

    let fetchSnapshot (storageLocation: string) =
        let ladderJson = StorageIO.download( Uri(storageLocation) )
        JsonConvert.DeserializeObject<TeamInfo list> ladderJson

    let ladder = fetchSnapshot url

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

        ladder 
        |> Seq.filter (fun t -> t.matchesFilter fotmFilters)


