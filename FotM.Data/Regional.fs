namespace FotM.Data

type RegionalSettings = {
    code: string;
    blizzardApiRoot: string;
    storageConnection: string;
    serviceBusConnection: string;
    googleAnalyticsPropertyCode: string;
    googleAnalyticsPropertyWebSite: string;
}

module Regions = 

    let commonStorage = ""
    let commonServiceBus = ""

    let getHistoryPath region bracket snapshotId = sprintf "%s/%s/history/%A.json" region.code bracket.url snapshotId

    let getLadderPath region bracket snapshotId = sprintf "%s/%s/ladder/%A.json" region.code bracket.url snapshotId

    let US = {
        code = "US"
        blizzardApiRoot = "http://us.battle.net/api/wow/leaderboard/"
        storageConnection = commonStorage
        serviceBusConnection = commonServiceBus
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let EU = {
        code = "EU"
        blizzardApiRoot = "http://eu.battle.net/api/wow/leaderboard/"
        storageConnection = commonStorage
        serviceBusConnection = commonServiceBus
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""

    }

    let KR = {
        code = "KR"
        blizzardApiRoot = "http://kr.battle.net/api/wow/leaderboard/"
        storageConnection = commonStorage
        serviceBusConnection = commonServiceBus
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""

    }

    let TW = {
        code = "TW"
        blizzardApiRoot = "http://tw.battle.net/api/wow/leaderboard/"
        storageConnection = commonStorage
        serviceBusConnection = commonServiceBus
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""

    }

    let CN = {
        code = "CN"
        blizzardApiRoot = "http://www.battlenet.com.cn/api/wow/leaderboard/"
        storageConnection = commonStorage
        serviceBusConnection = commonServiceBus
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let all = [ US; EU; KR; TW; CN ]