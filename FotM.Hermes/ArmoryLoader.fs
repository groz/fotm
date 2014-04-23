namespace FotM.Hermes

open FSharp.Data
open Armory

type RegionCode = 
| US
| EU
| KR
| CN
| TW

type Region = {
    region: RegionCode;
    blizzardApiUrl: string;
    azureConnectionString: string;
}

type RawLadder = JsonProvider<"""{
    "rows" : [ {
    "ranking" : 1,
    "rating" : 2777,
    "name" : "Hamshamx",
    "realmId" : 11,
    "realmName" : "Tichondrius",
    "realmSlug" : "tichondrius",
    "raceId" : 2,
    "classId" : 7,
    "specId" : 264,
    "factionId" : 1,
    "genderId" : 1,
    "seasonWins" : 275,
    "seasonLosses" : 106,
    "weeklyWins" : 0,
    "weeklyLosses" : 0
    } ] 
    }
""">

type ArmoryPuller(region: Region, bracket: Bracket) =

    member this.toDomainPlayer(row: RawLadder.Row): PlayerEntry = {
            player = {
                        Player.name = row.Name;
                        Player.faction = enum row.FactionId;
                        Player.gender = enum row.GenderId
                        Player.race = enum row.RaceId;
                        Player.realm = {
                                            Realm.realmId = row.RealmId;
                                            Realm.realmName = row.RealmName;
                                            Realm.realmSlug = row.RealmSlug;
                                        };
                        Player.classSpec = Specs.toClass row.ClassId row.SpecId;
                     };
            ranking = row.Ranking;
            rating = row.Rating;
            seasonWins = row.SeasonWins;
            seasonLosses = row.SeasonLosses;
            weeklyWins = row.WeeklyWins;
            weeklyLosses = row.WeeklyLosses;
        }

    member this.load(): LadderSnapshot =
        let rawLadder = RawLadder.Load(region.blizzardApiUrl + bracket.url)

        let ladder = rawLadder.Rows |> Seq.map (fun row -> this.toDomainPlayer(row))

        { bracket = bracket; ladder = ladder; timeTaken = NodaTime.SystemClock.Instance.Now }