namespace FotM.Hephaestus

open System
open System.Diagnostics
open CollectionExtensions
 
module TraceLogging = 
    let log trace message = message |> Printf.ksprintf (fun msg -> trace(sprintf "[%A] %s" DateTime.UtcNow msg))
    let logInfo message = message |> log Trace.TraceInformation
    let logError message = message |> log Trace.TraceError
    let logWarning message = message |> log Trace.TraceWarning

    let logException fmt (ex: Exception) = 

        let stack = 
            ex |> Seq.unfold(fun currentEx ->
                match currentEx |> asOption with
                | None -> None
                | Some(ex) -> Some(ex.Message, ex.InnerException)
             )

        let combinedMessage = String.Join(", Inner exception: ", stack)

        combinedMessage |> logError fmt