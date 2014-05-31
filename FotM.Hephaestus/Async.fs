namespace FotM.Hephaestus

open System

module Async =
    type Agent<'T> = MailboxProcessor<'T>

    type RetryCountExceededException(innerException: exn) =
        inherit Exception("Retry count exceeded", innerException)

    type RetryBuilder(max, ?logError: Exception -> unit) = 

      member x.Return(a) = a               // Enable 'return'

      member x.Delay(f) = f                // Gets wrapped body and returns it (as it is)
                                           // so that the body is passed to 'Run'

      member x.Zero() = x.Return( () )   // Support if .. then 

      member x.Run(f) =                    // Gets function created by 'Delay'

        let rec loop (n: int) (ex: exn option) = 
          if n <= 0 then // Number of retries exceeded
            match ex with
            | Some(e) -> raise (RetryCountExceededException e)
            | None -> failwith "Number of retries exceeded"
          else 
            try 
                f() 
            with 
            | ex ->
                if (logError.IsSome) then logError.Value(ex)
                loop (n-1) (Some ex)

        loop max None
