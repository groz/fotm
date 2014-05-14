namespace FotM.Argus

(*
Workflow of this module:
    - Constantly scan all brackets of all armory regions
    - If the current update wasn't seen yet
        - Save it in Azure storage
        - TODO: Post update message with storage location to an Azure topic
*)

open System.Threading
open System.Net
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.ServiceRuntime
open FotM.Hephaestus.TraceLogging
open FotM.Aether

type WorkerRole() =
    inherit RoleEntryPoint() 
    
    let cts = new CancellationTokenSource()

    override wr.Run() = 
        let serviceBus = ServiceBus()
        let publisher = serviceBus.topic "updates"
        Async.RunSynchronously(Argus.watch publisher, cancellationToken = cts.Token)

    override wr.OnStart() = 
        ServicePointManager.DefaultConnectionLimit <- 12
        base.OnStart()

    override wr.OnStop() =
        logInfo "Graceful shutdown initiated. Cancellation signaled..."
        cts.Cancel()
        cts.Dispose()
