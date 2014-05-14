namespace FotM.Athena

(*
Workflow of this module:
    TODO:
    - Subscribe to topic updates from Argus
    - Calculate teams for update
    - Post calculated teams to appropriate topic
*)

open System
open System.Threading
open System.Net
open Microsoft.WindowsAzure.ServiceRuntime
open FotM.Hephaestus.TraceLogging
open FotM.Aether

type WorkerRole() =
    inherit RoleEntryPoint() 

    let waitHandle = new ManualResetEvent(false)

    override wr.Run() = 
        let serviceBus = ServiceBus()

        let subscriptionName = 
            if RoleEnvironment.IsEmulated then
                Dns.GetHostName()
            else
                Dns.GetHostName() + Guid.NewGuid().ToString().Substring(0, 4)

        let updateTopic = serviceBus.subscribe "updates" subscriptionName

        let updatePublisher = serviceBus.topic "ladders"

        AthenaProcessor.watch updateTopic updatePublisher waitHandle

    override wr.OnStart() = 
        ServicePointManager.DefaultConnectionLimit <- 12
        base.OnStart()

    override wr.OnStop() =
        logInfo "Graceful shutdown initiated. Cancellation signaled..."
        waitHandle.Set() |> ignore
        waitHandle.Dispose()
