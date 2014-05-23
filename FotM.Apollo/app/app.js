console.log("app.js");

var app = angular.module('app', ['ngRoute']);

var sharedProperties = {
    currentRegion: "US",

    regions: ["us", "eu", "kr", "tw", "cn"],

    brackets: {
        "2v2": 2,
        "3v3": 3,
        "5v5": 5,
        "rbg": 10
    }
};