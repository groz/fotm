module FotM.Hephaestus.Tests.GATests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FotM.Hephaestus.GoogleAnalytics

[<TestClass>]
type ``Google Analytics client tests``() = 

    let propertyId = "UA-49247455-4"
    let clientId() = "123"

    [<TestMethod>]
    member this.``just fire few requests``() = 
        let ev = {
            category = "someev"
            action = "someact"
            label = "somelabel"
            value = "1"
        }

        for i in seq {0..10} do
           let responseCode = sendEvent propertyId (clientId()) ev
           printfn "%i" responseCode
           System.Threading.Thread.Sleep(100)
