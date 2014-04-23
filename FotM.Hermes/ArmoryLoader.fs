namespace FotM.Hermes

open FSharp.Data
open FotM.Data

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

type ArmoryLoader(region: RegionalSettings, bracket: Bracket) =

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
        let rawLadder = RawLadder.Load(region.blizzardApiRoot + bracket.url)

        { 
            bracket = bracket; 
            ladder = rawLadder.Rows |> Seq.map this.toDomainPlayer;
            timeTaken = NodaTime.SystemClock.Instance.Now 
        }