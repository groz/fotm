app.provider("shared", function () {
    var sharedProperties = {
        lastPage: "",
        currentRegion: "US",
        currentBracket: { text: "3v3"},

        regions: ["us", "eu", "kr", "tw", "cn"],

        brackets: {
            "2v2": 2,
            "3v3": 3,
            "5v5": 5,
            "rbg": 10
        },

        regionLink: function(region) {
            var url = '/' + region + '/' + this.currentBracket.text;
            if (this.now)
                url = url + "/now";
            return url;
        },

        bracketLink: function(bracket) {
            var url = '/' + this.currentRegion + '/' + bracket;
            if (this.now)
                url = url + "/now";
            return url;
        },

        redirectPage: function () {
            console.log("REDIRECTING TO LAST PAGE:", this.lastPage);
            if (this.lastPage) return this.lastPage;
            else return "/us/3v3";
        }
    };

    this.$get = function ($cookies) {
        if ($cookies)
            sharedProperties.lastPage = $cookies.lastPage;

        return sharedProperties;
    };

    this.$get.$inject = ['$cookies']; // minification protection
});