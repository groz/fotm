namespace FotM.Data

type RegionalSettings = {
    code: string;
    blizzardApiRoot: string;
}

module Regions = 

    let snapshotsContainer = "snapshots"

    let laddersContainer = "ladders"

    let googleAnalyticsPropertyCode = ""

    let googleAnalyticsPropertyWebSite = ""

    let US = {
        code = "US"
        blizzardApiRoot = "http://us.battle.net/api/wow/leaderboard/"
    }

    let EU = {
        code = "EU"
        blizzardApiRoot = "http://eu.battle.net/api/wow/leaderboard/"
    }

    let KR = {
        code = "KR"
        blizzardApiRoot = "http://kr.battle.net/api/wow/leaderboard/"
    }

    let TW = {
        code = "TW"
        blizzardApiRoot = "http://tw.battle.net/api/wow/leaderboard/"
    }

    let CN = {
        code = "CN"
        blizzardApiRoot = "http://www.battlenet.com.cn/api/wow/leaderboard/"
    }

    let all = [ US; EU; KR; TW; CN ]