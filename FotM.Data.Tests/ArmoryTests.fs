module FotM.Data.Tests.ArmoryTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FotM.Data

module ArmoryData =
    let groz = {
        Player.classSpec = Warrior(Some WarriorSpec.Fury)
        Player.faction = Faction.Alliance
        Player.gender = Gender.Male
        Player.name = "Groz"
        Player.race = Race.Human
        Player.realm = 
            {
                realmId = 1
                realmName = "Soulflayer"
                realmSlug = "soulflayer"
            }
    }

    let srez = {
        Player.classSpec = Druid(Some DruidSpec.Restoration)
        Player.faction = Faction.Alliance
        Player.gender = Gender.Female
        Player.name = "Srez"
        Player.race = Race.``Night Elf``
        Player.realm = 
            {
                realmId = 1
                realmName = "Soulflayer"
                realmSlug = "soulflayer"
            }
    }

    let teamEntry = {
        players = [srez; groz]
        ratingChange = 15
        rating = 2000
        snapshotTime = NodaTime.SystemClock.Instance.Now
    }

[<TestClass>]
type ``TeamEntry tests``() =
    [<TestMethod>]
    member this.``TeamEntry getClasses should return correct classes in correct order`` () =            
        let output = ArmoryData.teamEntry.getClasses()  |> Seq.toArray
        CollectionAssert.AreEqual([| Warrior(Some WarriorSpec.Fury); Druid(Some DruidSpec.Restoration) |], output)

    [<TestMethod>]
    member this.``TeamEntry getPlayers should return correct players in correct order`` () =
        let output = ArmoryData.teamEntry.getPlayers() |> Seq.toArray
        CollectionAssert.AreEqual([| ArmoryData.groz; ArmoryData.srez |], output)

//let createEntry (snapshotTime: NodaTime.Instant) (playerUpdates: PlayerUpdate list) = {
[<TestClass>]
type ``createEntry tests``() =
    let grozUpdate = 
        {
            player = ArmoryData.groz
            ranking = 10
            rating = 2000
            weeklyWins = 10
            weeklyLosses = 10
            seasonWins = 100
            seasonLosses = 100
            ratingDiff = 10
        }
    let srezUpdate = 
        {
            player = ArmoryData.srez
            ranking = 20
            rating = 1900
            weeklyWins = 10
            weeklyLosses = 10
            seasonWins = 100
            seasonLosses = 100
            ratingDiff = 20
        }

    [<TestMethod>]
    member this.``createEntry should create teamEntry with correct fields`` () =

        let now = NodaTime.SystemClock.Instance.Now

        let teamEntry = Teams.createEntry now [grozUpdate; srezUpdate]
        
        let expected = {
            players = [ArmoryData.groz; ArmoryData.srez]
            ratingChange = 15
            rating = 1950
            snapshotTime = now
        }

        Assert.AreEqual(expected.players, teamEntry.players)

    [<TestMethod>]
    member this.``createTeamInfo should create correct teamInfo for given team entries`` () =
        
        let t1 = NodaTime.SystemClock.Instance.Now
        let t1Entry = Teams.createEntry t1 [grozUpdate; srezUpdate]

        let duration = NodaTime.Duration.FromMinutes 3L
        let t2 = t1 + duration
        let t2Entry = Teams.createEntry t2 [grozUpdate; srezUpdate]

        let expected = {
            lastEntry = t2Entry
            totalWins = 2
            totalLosses = 0
        }

        Assert.AreEqual(expected, Teams.createTeamInfo [t1Entry; t2Entry])