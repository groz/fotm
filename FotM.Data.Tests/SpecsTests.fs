module FotM.Data.Tests.SpecsTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FotM.Data


//  let toSpecEnum<'a when 'a: enum<int>>(value: int): 'a option =
[<TestClass>]
type ``toSpecEnum tests``() =
    [<TestMethod>]
    member this.``toSpecEnum should correctly transform int to corresponding enum value`` () = 
        let c = Paladin(Specs.toSpecEnum 70)        
        Assert.AreEqual(Paladin(Some PaladinSpec.Retribution), c)

    [<TestMethod>]
    member this.``toSpecEnum should output None for non-existing int value`` () = 
        let c = Paladin(Specs.toSpecEnum 7220)
        Assert.AreEqual(Paladin(None), c)

// let toClassOption classId specId = 
[<TestClass>]
type ``toClassOption tests``() =
    [<TestMethod>]
    member this.``toClassOption should correctly transform ids to corresponding class value`` () = 
        let c = Specs.toClassOption 2 70
        Assert.AreEqual(Some( Paladin(Some PaladinSpec.Retribution) ), c)

    [<TestMethod>]
    member this.``toClassOption should output None for unknown ids`` () = 
        let c = Specs.toClassOption 99 70
        Assert.AreEqual(None, c)

//let toClass classId specId = 
[<TestClass>]
type ``toClass tests``() =
    [<TestMethod>]
    member this.``toClass should correctly transform ids to corresponding class value`` () = 
        let c = Specs.toClass 2 70
        Assert.AreEqual(Paladin(Some PaladinSpec.Retribution), c)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member this.``toClass should throw exception for unknown ids`` () = 
        let c = Specs.toClass 99 70
        Assert.AreEqual(Paladin(Some PaladinSpec.Retribution), c)

//let fromString className (specIdStr: string) =
[<TestClass>]
type ``fromString tests``() =
    [<TestMethod>]
    member this.``fromString should correctly transform strings to corresponding class`` () = 
        let c = Specs.fromString "Warrior" "71"
        Assert.AreEqual(Some(Warrior(Some WarriorSpec.Arms)), c)

    [<TestMethod>]
    member this.``fromString should return None for unknown settings`` () = 
        let c = Specs.fromString "Unknown" "71"
        Assert.AreEqual(None, c)

//let getClassId = 
[<TestClass>]
type ``getClassId tests``() =
    [<TestMethod>]
    member this.``getClassId should return appropriate classId`` () =
        let c = Paladin None
        Assert.AreEqual(2, Specs.getClassId c)

[<TestClass>]
type ``defined tests``() =
    [<TestMethod>]
    member this.``Non-None class should be defined`` () =
        let c = Paladin(Some PaladinSpec.Holy)
        Assert.IsTrue(c.defined)

    [<TestMethod>]
    member this.``Class with None spec should be undefined`` () =
        let c = Paladin(None)
        Assert.IsFalse(c.defined)

[<TestClass>]
type ``isHealer tests``() =
    [<TestMethod>]
    member this.``Warriors can't be healers`` () =
        let c = Warrior(None)
        Assert.IsFalse(c.isHealer)

    [<TestMethod>]
    member this.``Discipline priest is a healer`` () =
        let c = Priest(Some PriestSpec.Discipline)
        Assert.IsTrue(c.isHealer)

[<TestClass>]
type ``isRanged tests``() =
    [<TestMethod>]
    member this.``Warriors can't be ranged`` () =
        let c = Warrior(Some WarriorSpec.Arms)
        Assert.IsFalse(c.isRanged)

    [<TestMethod>]
    member this.``Shadow priest is ranged dd`` () =
        let c = Priest(Some PriestSpec.Shadow)
        Assert.IsTrue(c.isRanged)