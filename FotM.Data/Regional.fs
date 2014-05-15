namespace FotM.Data

type RegionalSettings = {
    code: string;
    blizzardApiRoot: string;
    googleAnalyticsPropertyCode: string;
    googleAnalyticsPropertyWebSite: string;
}

module Regions = 

    let snapshotsContainer = "snapshots"

    let laddersContainer = "ladders"

    let US = {
        code = "US"
        blizzardApiRoot = "http://us.battle.net/api/wow/leaderboard/"
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let EU = {
        code = "EU"
        blizzardApiRoot = "http://eu.battle.net/api/wow/leaderboard/"
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let KR = {
        code = "KR"
        blizzardApiRoot = "http://kr.battle.net/api/wow/leaderboard/"
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""

    }

    let TW = {
        code = "TW"
        blizzardApiRoot = "http://tw.battle.net/api/wow/leaderboard/"
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let CN = {
        code = "CN"
        blizzardApiRoot = "http://www.battlenet.com.cn/api/wow/leaderboard/"
        googleAnalyticsPropertyCode = ""
        googleAnalyticsPropertyWebSite = ""
    }

    let all = [ US; EU; KR; TW; CN ]