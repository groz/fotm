namespace FotM.Apollo.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax

type DebugController() =
    inherit Controller()

    member this.Index () = 
        this.View()