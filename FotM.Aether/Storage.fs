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
open FotM.Hephaestus.TraceLogging
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

    member this.upload (data, ?path) =
        let relativePath = defaultArg path (sprintf "%A" (Guid.NewGuid()))
        let blob = container.GetBlockBlobReference (Path.Combine (prefix, relativePath))

        try
            lock RepoSync.lockobj (fun () ->
                let json = JsonConvert.SerializeObject data // serialization is not threadsafe here
                let compressed = CompressionUtils.ZipToBase64 json
                blob.UploadText compressed 
            )
        with
            | ex -> 
                logError "%A" ex
                reraise()

        logInfo "uploaded update to %A" blob.Uri

        blob.Uri

module StorageIO =
    let download (storageLocation: Uri) =
        logInfo "Fetching %A" storageLocation
        use webClient = new WebClient()
        webClient.Encoding <- Encoding.UTF8
        let compressed = webClient.DownloadString storageLocation
        let json = CompressionUtils.UnzipFromBase64 compressed
        json