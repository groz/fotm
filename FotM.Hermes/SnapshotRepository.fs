namespace FotM.Hermes

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Newtonsoft.Json
open NodaTime.Serialization.JsonNet
open NodaTime
open FotM.Data


type UploadRequestMessage = LadderSnapshot * AsyncReplyChannel<Uri>

type SnapshotRepository(region: RegionalSettings, bracket: Bracket) =

    let storageAccount = CloudStorageAccount.Parse region.storageConnection

    let blobClient = storageAccount.CreateCloudBlobClient()

    let containerName = sprintf "snapshots"
    let container = blobClient.GetContainerReference containerName

    do
        printfn "initializing blob container at %A" container.Uri
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore

    let uploadSnapshot snapshot = 
        let snapshotId = Guid.NewGuid()
        let blobName = sprintf "%s/%s/%A.json" region.code bracket.url snapshotId

        let blob = container.GetBlockBlobReference(blobName)

        try
            let json = JsonConvert.SerializeObject(snapshot)
            blob.UploadText(json)
        with
            | :? System.NullReferenceException -> 
                printfn "NullRef exception. Race condition while uploading."
            | ex ->
                printfn "Exception occured while uploading: %A" ex

        blob.Uri


    member this.uploader = MailboxProcessor<UploadRequestMessage>.Start(fun inbox-> 
            async { 
                while true do        
                    let! (msg, replyChannel) = inbox.Receive()
                    let uri = uploadSnapshot msg
                    replyChannel.Reply(uri)
            }
    )
