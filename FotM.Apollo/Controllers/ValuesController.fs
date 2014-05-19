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
[<RoutePrefix("api2/values")>]
type ValuesController() =
    inherit ApiController()

    let url = @"http://127.0.0.1:10000/devstoreaccount1/ladders/US/3v3/01ad5450-2968-4b35-a8a5-13a8d415de13"

    let fetchSnapshot (storageLocation: string) =
        let ladderJson = StorageIO.download( Uri(storageLocation) )
        JsonConvert.DeserializeObject<TeamInfo list> ladderJson

    let ladder = fetchSnapshot url

    /// Gets all values.
    [<Route("")>]
    member this.Get() = 
        ladder

    /// Gets the value with index id.
    (*[<Route("{id:int}")>]
    member x.Get(id) : IHttpActionResult =
        if id > values.Length - 1 then
            x.BadRequest() :> _
        else x.Ok(values.[id]) :> _
    *)