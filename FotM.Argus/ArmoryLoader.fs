﻿namespace FotM.Argus

open FSharp.Data
open FotM.Data
open NodaTime
open FotM.Hephaestus.TraceLogging

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

module ArmoryLoader =

    let toDomainPlayer(rank: int, row: RawLadder.Row): PlayerEntry = {
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
            ranking = rank;
            rating = row.Rating;
            seasonWins = row.SeasonWins;
            seasonLosses = row.SeasonLosses;
            weeklyWins = row.WeeklyWins;
            weeklyLosses = row.WeeklyLosses;
        }

    let loadAsync(region: RegionalSettings) (bracket: Bracket) = async {
        logInfo "Loading ladder for region %s bracket %s..." region.code bracket.url

        let! rawLadder = RawLadder.AsyncLoad(region.blizzardApiRoot + bracket.url)

        logInfo "Ladder for region %s bracket %s loaded successfully." region.code bracket.url

        // apply consistent ordering to the ladder
        let ladder =
            rawLadder.Rows 
            |> Array.sortBy (fun row -> -row.Rating, row.Name, row.RealmId) 
            |> Array.mapi (fun i row -> toDomainPlayer(i + 1, row))
            |> Seq.take 1000 // TODO: revert this workaround for clustering algo limitation
            |> Seq.toArray

        return {
            region = region.code
            bracket = bracket
            ladder = ladder
            timeTaken = SystemClock.Instance.Now
        }
    }

    let load(region: RegionalSettings) (bracket: Bracket) = loadAsync region bracket |> Async.RunSynchronously