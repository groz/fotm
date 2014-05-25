namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax


type MetaInformation = {
    name: string
    title: string
    description: string
    rootUrl: string
    previewUrl: string
}

type HomeController() =
    inherit Controller()

    let root = "http://fotm-info.azurewebsites.net"

    let metaInfo = {
        name = "FotM"
        title = "Flavor of the Month team ratings for World of Warcraft"
        description = "Website that shows currently popular team combinations for World of Warcraft arena. It allows for filtering of setups to help finding best one for your team composition and brings the feel of old single-team arenas back."
        rootUrl = root
        previewUrl = root + "/preview.png"
    }

    member this.Index () = 
        this.View metaInfo