namespace FotM.Apollo

open System.Web.Configuration
open System.Net
open FotM.Aether
open FotM.Data

module Main =

    let mutable latestUpdate: FotM.Aether.UpdateMesage option = None
    let mutable currentState = ()

    let Startup(): unit = 

    (*
        let storageConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.Storage.ConnectionString"]
        let serviceBusConnectionString = WebConfigurationManager.ConnectionStrings.["Microsoft.ServiceBus.ConnectionString"]
        
        let storage = Storage(GlobalSettings.teamLaddersContainer, storageConnectionString.ConnectionString)
        let serviceBus = ServiceBus(serviceBusConnectionString.ConnectionString)

        let subscriptionName = Dns.GetHostName()

        let updateTopic = serviceBus.subscribe GlobalSettings.playerUpdatesTopic subscriptionName

        updateTopic.OnMessage(fun msg ->
            let updateMessage = msg.GetBody<UpdateMesage>()

            latestUpdate <- Some(updateMessage)

            msg.Complete()
        )
    *)
        ()
