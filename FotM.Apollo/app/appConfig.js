app.config(function ($routeProvider, sharedProvider, $locationProvider) {
    // this is an example of how you can write unreadable javascript with any framework :(

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

    sharedProvider.set(sharedProperties);

    function createSettingsProvider(r, b) {

        var provider = function () {
            return {
                region: r,
                bracket: {
                    text: b,
                    size: brackets[b]
                }
            };
        };

        return provider;

    }

    var routeProvider = $routeProvider;

    var regions = sharedProperties.regions;
    var brackets = sharedProperties.brackets;

    for (var ir in regions) {
        var region = regions[ir];
        var regionRoot = '/' + region;

        for (var bracket in brackets) {

            var url = '/' + region + '/' + bracket;

            var settingsProvider = createSettingsProvider(region, bracket, false);

            routeProvider
                .when(url,
                {
                    controller: "LeaderboardController",
                    templateUrl: "app/templates/" + bracket + ".html",
                    resolve: {
                        settings: settingsProvider
                    }
                })
                .when(url + '/now',
                {
                    controller: "NowController",
                    templateUrl: "app/templates/" + bracket + "_now.html",
                    resolve: {
                        settings: settingsProvider
                    }
                });
        }

        routeProvider
            .when(regionRoot,
            {
                redirectTo: regionRoot + "/3v3"
            });
    }

    routeProvider
        .when("/about",
        {
            controller: "AboutController",
            templateUrl: "app/templates/about.html"
        })
        .otherwise({ redirectTo: '/us/3v3' });

    // use HTML5 style links without #
    // it will fallback automatically to # routes on older browsers
    $locationProvider.html5Mode(true);
});

app.run(function ($rootScope, shared) {
    $rootScope.shared = shared; // getting shared properties into global scope
});