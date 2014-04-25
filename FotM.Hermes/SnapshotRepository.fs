namespace FotM.Hermes

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open NodaTime.Serialization.JsonNet
open NodaTime
open FotM.Data

type SnapshotRepository(region: RegionalSettings, bracket: Bracket) =

    let storageAccount = CloudStorageAccount.Parse region.storageConnection

    let blobClient = storageAccount.CreateCloudBlobClient()

    let containerName = sprintf "snapshots"
    let container = blobClient.GetContainerReference containerName

    do
        printfn "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    member this.uploadSnapshot snapshot = 
        let snapshotId = Guid.NewGuid()
        let blobName = sprintf "%s/%s/%A.json" region.code bracket.url snapshotId

        let blob = container.GetBlockBlobReference(blobName)

        try
            let settings = JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
            let json = JsonConvert.SerializeObject(snapshot, settings)
            blob.UploadText(json)
        with
            | :? System.NullReferenceException -> 
                printfn "*********** ERROR *****************"
                let guid = Guid.NewGuid()
                let filename = sprintf "error_%A.txt" guid
                printfn "NullRef Exception occured. Storing snapshot in file %s" filename
                let str = sprintf "%A" snapshot
                System.IO.File.WriteAllText(filename, str)

        blob.Uri


