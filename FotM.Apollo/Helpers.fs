namespace FotM.Apollo

open System.Net.Http.Formatting
open System.Net.Http.Headers
open Newtonsoft.Json
open System.Threading.Tasks

// http://weblog.west-wind.com/posts/2012/Mar/09/Using-an-alternate-JSON-Serializer-in-ASPNET-Web-API

type JsonNetFormatter() =
    inherit MediaTypeFormatter()

    do
        base.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"))
        base.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"))

    let writeLock = obj()

    let write value (stream: System.IO.Stream) =
        lock writeLock (fun () ->
            let json = JsonConvert.SerializeObject(value, Formatting.Indented)
            use writer = new System.IO.StreamWriter(stream)
            writer.Write json
            writer.Flush()
        )

    let read t (stream: System.IO.Stream) =
        use reader = new System.IO.StreamReader(stream)
        use jreader = new JsonTextReader(reader)
        let ser = JsonSerializer()
        try
            ser.Deserialize(jreader, t)
        with
        | ex -> null

    override this.CanWriteType t =
        true

    override this.CanReadType t =
        true

    override this.ReadFromStreamAsync (t, stream, content, logger) =
        let readFunc = async { return read t stream } 
        Async.StartAsTask(readFunc)

    override this.ReadFromStreamAsync (t, stream, content, logger, token) =
        let readFunc = async { return read t stream } 
        Async.StartAsTask(readFunc, cancellationToken = token)

    override this.WriteToStreamAsync (t, value, stream, content, ctx) =
        Task.Factory.StartNew (fun () -> write value stream)

    override this.WriteToStreamAsync (t, value, stream, content, ctx, token) =
        Task.Factory.StartNew ((fun () -> write value stream), token)
