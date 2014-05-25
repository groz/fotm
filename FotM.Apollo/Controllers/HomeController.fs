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
        title = "FotM - flavor of the month team rating for World of Warcraft"
        description = "Website that shows flavor of the month team setups for World of Warcraft arena. Allows for search and filtering of setups to help you find the one that suits you the best and brings the feel of old single-team arenas back."
        rootUrl = root
        previewUrl = root + "/preview.png"
    }

    member this.Index () = 
        this.View metaInfo