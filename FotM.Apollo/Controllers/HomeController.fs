namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax

type IndexModel = {
    title: string
}    

type HomeController() =
    inherit Controller()

    member this.Index () = 
        let model = { title = "IndexModelTitle "}
        this.View(model)