namespace FotM.Hermes

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open FotM.Data

type SnapshotRepository(region: RegionalSettings, bracket: Bracket) =

    let storageAccount = CloudStorageAccount.Parse region.storageConnection

    let blobClient = storageAccount.CreateCloudBlobClient()

    let containerName = sprintf "%s-snapshots" (region.code.ToLower())
    let container = blobClient.GetContainerReference containerName

    do
        printfn "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    member this.uploadSnapshot snapshot = 
        let snapshotId = Guid.NewGuid()
        let blobName = sprintf "%s/%A.json" bracket.url snapshotId

        let blob = container.GetBlockBlobReference(blobName)
        blob.UploadText(JsonConvert.SerializeObject snapshot)

        printfn "Snapshot uploaded to %s" (blob.Uri.ToString())
        snapshotId // return id


