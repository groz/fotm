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

    let url = @"http://127.0.0.1:10000/devstoreaccount1/ladders/EU/3v3/9ebad072-7f18-4853-a29c-8e460cb93182"

    let fetchSnapshot (storageLocation: string) =
        let ladderJson = StorageIO.download( Uri(storageLocation) )
        JsonConvert.DeserializeObject<TeamInfo list> ladderJson

    let ladder = fetchSnapshot url

    let passes (c: Class) (f: Class) = 
        match f with
        | Warrior(None) -> match c with | Warrior(_) -> true | _ -> false
        | Paladin(None) -> match c with | Paladin(_) -> true | _ -> false
        | Hunter(None) -> match c with | Hunter(_) -> true | _ -> false
        | Rogue(None) -> match c with | Rogue(_) -> true | _ -> false
        | Priest(None) -> match c with | Priest(_) -> true | _ -> false
        | ``Death Knight``(None) -> match c with | ``Death Knight``(_) -> true | _ -> false
        | Shaman(None) -> match c with | Shaman(_) -> true | _ -> false
        | Mage(None) -> match c with | Mage(_) -> true | _ -> false
        | Warlock(None) -> match c with | Warlock(_) -> true | _ -> false
        | Monk(None) -> match c with | Monk(_) -> true | _ -> false
        | Druid(None) -> match c with | Druid(_) -> true | _ -> false
        | _  -> c = f

    (* Outputs true if for each class/spec in teamSetup the number or occurences >= those in filters *)
    let passes (teamSetup: Class array) (filters: Class array) =
        let passingPlayers (f: Class) =
            teamSetup
            |> Seq.filter (fun c -> passes c f)
            |> Seq.length
        
        let passingFilters =
            filters
            |> Seq.countBy (fun c -> c)
            |> Seq.filter (fun (c, number) -> (passingPlayers c) >= number)

        Seq.length(passingFilters) = Array.length(filters)

    let filter filters (teams: TeamInfo list) =
        teams
        |> Seq.filter (fun t -> 
            let specs = t.lastEntry.players |> Seq.map(fun p -> p.classSpec) |> Array.ofSeq
            passes specs filters
        )

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

        ladder |> filter fotmFilters


