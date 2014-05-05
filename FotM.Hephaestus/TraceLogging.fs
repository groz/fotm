namespace FotM.Hephaestus

open System
open System.Diagnostics

module TraceLogging = 
    let log trace message = message |> Printf.ksprintf (fun msg -> trace(sprintf "[%A] %s" DateTime.UtcNow msg))
    let logInfo (message: 'a) = message |> log Trace.TraceInformation
    let logError (message: 'a) = message |> log Trace.TraceError
    let logWarning (message: 'a) = message |> log Trace.TraceWarning