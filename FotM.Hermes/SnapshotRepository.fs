namespace FotM.Hermes

open System
open System.IO
open System.Threading
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open NodaTime.Serialization.JsonNet
open NodaTime
open FotM.Data

module RepoSync =
    let lockobj = new System.Object()

type SnapshotRepository(region: RegionalSettings, bracket: Bracket) =

    let storageAccount = CloudStorageAccount.Parse region.storageConnection

    let blobClient = storageAccount.CreateCloudBlobClient()

    let container = blobClient.GetContainerReference Regions.snapshotsContainer

    do
        printfn "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    member this.uploadSnapshot snapshot = 
        let snapshotId = Guid.NewGuid()
        let blobName = Regions.getPath region bracket snapshotId

        let blob = container.GetBlockBlobReference(blobName)

        try
            lock RepoSync.lockobj (fun () ->
                blob.UploadText (JsonConvert.SerializeObject snapshot) // serialization is not threadsafe here
            )
        with
            | :? System.NullReferenceException -> 
                printfn "NullRef exception. Race condition while uploading."
            | ex ->
                printfn "Exception occured while uploading: %A" ex

        blob.Uri