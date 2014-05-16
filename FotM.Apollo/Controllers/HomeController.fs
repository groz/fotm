namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax

type IndexModel = {
    title: Uri
}    

type HomeController() =
    inherit Controller()

    member this.Index () = 
        let model = { title = FotM.Apollo.Main.latestUpdate.Value.storageLocation }
        this.View(model)