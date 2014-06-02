module FotM.Data.Tests.TeamsTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FotM.Data

[<TestClass>]
type ``matchesFilter tests``() =
// module Teams =
//  let matchesFilter (classFilters: Class array) (teamClasses: Class seq) =

    let warrior = Warrior(None)
    let armsWarrior = Warrior(Some WarriorSpec.Arms)
    let furyWarrior = Warrior(Some WarriorSpec.Fury)
    let afflictionWarlock = Warlock(Some WarlockSpec.Affliction)
    let restoShaman = Shaman(Some ShamanSpec.Restoration)

    [<TestMethod>]
    member this.``matchesFilter should match correct single class`` () = 
        let classes = [| armsWarrior |]
        let filters = [| armsWarrior |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should not match incorrect single class`` () = 
        let classes = [| armsWarrior |]
        let filters = [| furyWarrior |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match single class if no spec is specified on filter`` () = 
        let classes = [| armsWarrior |]
        let filters = [| Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match single class with no spec if no spec is specified on filter`` () = 
        let classes = [| warrior |]
        let filters = [| Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should not match single class with no spec if spec is specified on filter`` () = 
        let classes = [| warrior |]
        let filters = [| armsWarrior |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team for fully specified filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| armsWarrior; afflictionWarlock; restoShaman |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team for incompletely specified filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| armsWarrior; restoShaman |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should fail matching for some filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| furyWarrior; afflictionWarlock; restoShaman |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should fail matching for some wildcard filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| Warrior(None); afflictionWarlock; Druid(None) |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team for full wildcard filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| Warrior(None); Shaman(None); Warlock(None); |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )
        
    [<TestMethod>]
    member this.``matchesFilter should match team for incomplete wildcard filters`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| Shaman(None); Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should fail for stacked filter`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| armsWarrior; armsWarrior |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should fail for stacked wildcard filter`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| Warrior(None); Warrior(None) |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team with stacked classes`` () = 
        let classes = [| armsWarrior; furyWarrior; restoShaman |]
        let filters = [| Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team with stacked specs and wildcard filter`` () = 
        let classes = [| armsWarrior; armsWarrior; restoShaman |]
        let filters = [| Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should fail for mixed stacked wildcard filter`` () = 
        let classes = [| armsWarrior; afflictionWarlock; restoShaman |]
        let filters = [| armsWarrior; Warrior(None) |]
        Assert.IsFalse( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team with stacked classes and stacked wildcard filter`` () = 
        let classes = [| armsWarrior; furyWarrior; restoShaman |]
        let filters = [| Warrior(None); Warrior(None) |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )

    [<TestMethod>]
    member this.``matchesFilter should match team with stacked specs and stacked wildcard filter`` () = 
        let classes = [| armsWarrior; armsWarrior; restoShaman |]
        let filters = [| armsWarrior; armsWarrior |]
        Assert.IsTrue( classes |> Teams.matchesFilter filters )