namespace FotM.Data

type RegionalSettings = {
    blizzardApiRoot: string;
    storageConnection: string;
    serviceBusConnection: string;
    googleAnalyticsPropertyCode: string;
    googleAnalyticsPropertyWebSite: string;
}

module Regions = 

    let US = {
        blizzardApiRoot = "http://us.battle.net/api/wow/leaderboard/";
        storageConnection = "";
        serviceBusConnection = "";
        googleAnalyticsPropertyCode = "";
        googleAnalyticsPropertyWebSite = "";
    }

    let EU = {
        blizzardApiRoot = "http://eu.battle.net/api/wow/leaderboard/";
        storageConnection = "";
        serviceBusConnection = "";
        googleAnalyticsPropertyCode = "";
        googleAnalyticsPropertyWebSite = "";

    }

    let KR = {
        blizzardApiRoot = "http://kr.battle.net/api/wow/leaderboard/";
        storageConnection = "";
        serviceBusConnection = "";
        googleAnalyticsPropertyCode = "";
        googleAnalyticsPropertyWebSite = "";

    }

    let TW = {
        blizzardApiRoot = "http://tw.battle.net/api/wow/leaderboard/";
        storageConnection = "";
        serviceBusConnection = "";
        googleAnalyticsPropertyCode = "";
        googleAnalyticsPropertyWebSite = "";

    }

    let CN = {
        blizzardApiRoot = "http://www.battlenet.com.cn/api/wow/leaderboard/";
        storageConnection = "";
        serviceBusConnection = "";
        googleAnalyticsPropertyCode = "";
        googleAnalyticsPropertyWebSite = "";
    }