namespace FotM.Aether

open System
open System.IO
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open FotM.Data
open FotM.Hephaestus.TraceLogging

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
                blob.UploadText (JsonConvert.SerializeObject data) // serialization is not threadsafe here
            )
        with
            | ex -> 
                logError "%A" ex
                reraise()

        logInfo "uploaded update to %A" blob.Uri

        blob.Uri