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
        let model = { title = "Index" }
        this.View(model)

    member this.Mindex () = 
        let model = { title = "Mindex" }
        this.View("Index", model)