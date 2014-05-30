app.provider("shared", function () {
    var sharedProperties = {
        currentRegion: "US",

        currentBracket: {},

        regions: ["us", "eu", "kr", "tw", "cn"],

        brackets: {
            "2v2": 2,
            "3v3": 3,
            "5v5": 5,
            "rbg": 10
        },

        regionLink: function (region) {
            var url = '/' + region + '/' + this.currentBracket.text;
            if (this.now)
                url = url + "/now";
            return url;
        },

        bracketLink: function (bracket) {
            var url = '/' + this.currentRegion + '/' + bracket;
            if (this.now)
                url = url + "/now";
            return url;
        },
    };

    this.$get = function () { return sharedProperties; }
});