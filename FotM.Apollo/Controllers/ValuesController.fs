namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open System.Net
open Newtonsoft.Json
open FotM.Hephaestus.TraceLogging
open FotM.Data

type TeamList = TeamInfo list

/// Retrieves values.
[<RoutePrefix("api2/values")>]
type ValuesController() =
    inherit ApiController()

    let url = @"http://127.0.0.1:10000/devstoreaccount1/ladders/US/3v3/8779b5c5-6dcb-4439-87cb-8053dc13a286"

    let fetchSnapshot (storageLocation: string) =
        logInfo "Fetching ladder snapshot from %A" storageLocation
        use webClient = new WebClient()
        let ladderJson = webClient.DownloadString storageLocation
        JsonConvert.DeserializeObject<TeamList> ladderJson

    let ladder = fetchSnapshot url

    /// Gets all values.
    [<Route("")>]
    member x.Get() = ladder

    /// Gets the value with index id.
    (*[<Route("{id:int}")>]
    member x.Get(id) : IHttpActionResult =
        if id > values.Length - 1 then
            x.BadRequest() :> _
        else x.Ok(values.[id]) :> _
    *)