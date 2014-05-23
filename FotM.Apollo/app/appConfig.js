console.log("appConfig.js");

app.config(function ($routeProvider, sharedProvider) {

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

    sharedProvider.set(sharedProperties);

    // this is an example of how you can write unreadable javascript with any framework :(
    var routeProvider = $routeProvider;

    var regions = sharedProperties.regions;
    var brackets = sharedProperties.brackets;

    for (var ir in regions)
        for (var bracket in brackets) {

            var region = regions[ir];

            // binding closures to loop variables
            var regionProvider = function (r) { return function () { return r; }; }
            var bracketProvider = function (b) {
                return function () {
                    return {
                        text: b,
                        size: brackets[b]
                    };
                };
            }

            routeProvider = routeProvider
                .when('/' + region + '/' + bracket,
                {
                    controller: "ApiController",
                    templateUrl: "app/templates/" + bracket + ".html",
                    resolve: {
                        region: regionProvider(region),
                        bracket: bracketProvider(bracket)
                    }
                });

        };

    for (var ir in regions) {
        var region = regions[ir];

        var regionRoot = '/' + region;

        routeProvider = routeProvider
            .when(regionRoot,
            {
                redirectTo: regionRoot + "/3v3"
            })
            .when(regionRoot + '/:filter',
            {
                redirectTo: regionRoot + "/3v3"
            });
    }

    routeProvider.otherwise({ redirectTo: '/us/3v3' });
});

app.run(function ($rootScope, shared) {
    $rootScope.shared = shared; // getting shared properties into global scope
});
