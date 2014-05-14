namespace FotM.Aether

open System
open System.IO
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open FotM.Data

module RepoSync =
    let lockobj = new System.Object()

type Storage (containerName, ?storageConnectionString, ?pathPrefix) =

    let connectionString = defaultArg storageConnectionString (CloudConfigurationManager.GetSetting "Microsoft.Storage.ConnectionString")
    let prefix = defaultArg pathPrefix ""
    let storageAccount = CloudStorageAccount.Parse connectionString

    let blobClient = storageAccount.CreateCloudBlobClient()

    let container = blobClient.GetContainerReference containerName

    do
        printfn "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    member this.upload path data = 
        let blob = container.GetBlockBlobReference (Path.Combine (prefix, path))

        try
            lock RepoSync.lockobj (fun () ->
                blob.UploadText (JsonConvert.SerializeObject data) // serialization is not threadsafe here
            )
        with
            | :? System.NullReferenceException -> 
                printfn "NullRef exception. Race condition while uploading."
            | ex ->
                printfn "Exception occured while uploading: %A" ex

        blob.Uri