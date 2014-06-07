namespace FotM.Hephaestus.Tests.CompressionTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FotM.Hephaestus.Compression

[<TestClass>]
type ``Compression tests``() = 

    let data = "12345678901234567890"

    [<TestMethod>]
    member this.``zip and unzip should recreate the same object``() = 
        let zipped = zip data
        Assert.AreNotEqual(data, zipped)

        let unzipped = unzip zipped
        Assert.AreEqual(data, unzipped)
        
    [<TestMethod>]
    member this.``zip to base64 and unzip from base 64 should recreate the same object``() = 
        let zipped = zipToBase64 data
        Assert.AreNotEqual(data, zipped)

        let unzipped = unzipFromBase64 zipped
        Assert.AreEqual(data, unzipped)
