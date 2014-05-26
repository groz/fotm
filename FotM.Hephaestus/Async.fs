namespace FotM.Hephaestus

open System

module Async =
    type Agent<'T> = MailboxProcessor<'T>

    type RetryBuilder(max) = 
      let errorMessage = "Number of retries exceeded"
      member x.Return(a) = a               // Enable 'return'
      member x.Delay(f) = f                // Gets wrapped body and returns it (as it is)
                                           // so that the body is passed to 'Run'
      member x.Zero() = failwith errorMessage   // Support if .. then 
      member x.Run(f) =                    // Gets function created by 'Delay'
        let rec loop (n: int) (ex: Exception option) = 
          if n = 0 then // Number of retries exceeded
            match ex with
            | Some(e) -> raise e
            | None -> failwith errorMessage
          else 
            try f() with ex -> loop (n-1) (Some ex)
        loop max None
