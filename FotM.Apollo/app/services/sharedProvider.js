app.provider("shared", function () {
    var sharedProperties = {
        currentRegion: "US",

        currentBracket: "3v3",

        regions: ["us", "eu", "kr", "tw", "cn"],

        brackets: {
            "2v2": 2,
            "3v3": 3,
            "5v5": 5,
            "rbg": 10
        }
    };

    this.$get = function () { return sharedProperties; }
});