namespace FotM.Aether

open System
open System.IO
open System.Net
open System.Text
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open FotM.Data
open FotM.Hephaestus.Async
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.CollectionExtensions
open FotM.Utilities

module RepoSync =
    let lockobj = new System.Object()

type Storage (containerName, ?storageConnectionString, ?pathPrefix) =

    let connectionString = defaultArg storageConnectionString (CloudConfigurationManager.GetSetting "Microsoft.Storage.ConnectionString")
    let prefix = defaultArg pathPrefix ""
    let storageAccount = CloudStorageAccount.Parse connectionString

    let blobClient = storageAccount.CreateCloudBlobClient()

    let container = blobClient.GetContainerReference containerName

    do
        logInfo "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    member this.uploadFile (filePath: string, ?cacheTime: TimeSpan) =
        let relativePath = System.IO.Path.GetFileName filePath
        let targetPath = Path.Combine (prefix, relativePath)
        let blob = container.GetBlockBlobReference targetPath
        
        try
            blob.UploadFromFile(filePath, FileMode.Open)
            if cacheTime.IsSome then
                let cacheControlHeader = sprintf "max-age=%i" (int cacheTime.Value.TotalSeconds)
                blob.Properties.CacheControl <- cacheControlHeader
            blob.SetProperties()
        with
            | ex ->
                logException "%A" ex
                reraise()

        blob.Uri

    member this.upload (data, ?path) =
        let relativePath = defaultArg path (sprintf "%A" (Guid.NewGuid()))
        let targetPath = Path.Combine (prefix, relativePath)
        let blob = container.GetBlockBlobReference targetPath

        try
            // the part below is not threadsafe either because of JSON.NET or blob.UploadText
            lock RepoSync.lockobj (fun () ->
                let json = JsonConvert.SerializeObject data
                let compressed = CompressionUtils.ZipToBase64 json
                blob.UploadText compressed 
            )
        with
            | ex -> 
                logException "%A" ex
                reraise()

        logInfo "uploaded update to %A" blob.Uri

        blob.Uri

    member this.allBlobs(?directory) =
        let directory = defaultArg directory ""

        let allBlobs = 
            container.ListBlobs(prefix = directory, useFlatBlobListing = true)
            |> Seq.toArray

        allBlobs
        |> Array.choose(fun b -> 
            let blob = b :?> CloudBlockBlob
            match blob.Properties.LastModified |> toOption with
            | Some time -> Some(blob, time)
            | None -> None)
        |> Array.sortBy(fun (blob, t) -> t)
        |> Array.map(fun (blob, t) -> blob)

    member this.allFiles(?directory) =
        let directory = defaultArg directory ""

        this.allBlobs(directory) |> Array.map (fun blob -> blob.Uri)

type WebClientWithTimeout(timeout: TimeSpan) =
    inherit WebClient()

    override this.GetWebRequest(uri) =
        let wr = base.GetWebRequest(uri)
        wr.Timeout <- int timeout.TotalMilliseconds
        wr

module StorageIO =

    let retry = RetryBuilder(3)

    let downloadAsync (storageLocation: Uri) = async {
            logInfo "Fetching %A" storageLocation

            use webClient = new WebClientWithTimeout (TimeSpan.FromMinutes 2.0)
            let! compressed = webClient.AsyncDownloadString storageLocation

            let json = CompressionUtils.UnzipFromBase64 compressed
            return json
    }

    let download (storageLocation: Uri) = retry {
        let result = downloadAsync storageLocation |> Async.RunSynchronously
        return result
    }

    let fetchAsync<'a> storageLocation = async {
        let! snapshotJson = downloadAsync storageLocation
        return JsonConvert.DeserializeObject<'a> snapshotJson
    }

    let fetch<'a> storageLocation =
        let snapshotJson = download storageLocation
        JsonConvert.DeserializeObject<'a> snapshotJson