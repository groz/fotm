namespace FotM.Athena

(*
Workflow of this module:
    TODO:
    - Subscribe to topic updates from Argus
    - Calculate teams for update
    - Post calculated teams to appropriate topic
*)

open System.Threading
open System.Net
open Microsoft.WindowsAzure.ServiceRuntime
open FotM.Hephaestus.TraceLogging

type WorkerRole() =
    inherit RoleEntryPoint() 
    
    let cts = new CancellationTokenSource()

    override wr.Run() = 
        Async.RunSynchronously(Athena.watch, cancellationToken = cts.Token)

    override wr.OnStart() = 
        ServicePointManager.DefaultConnectionLimit <- 12
        base.OnStart()

    override wr.OnStop() =
        logInfo "Graceful shutdown initiated. Cancellation signaled..."
        cts.Cancel()
        cts.Dispose()
