app.config(['$routeProvider', 'sharedProvider', '$locationProvider', function ($routeProvider, sharedProvider, $locationProvider) {
    console.log("******* Configuring AngularJS app *******");
    var sharedProperties = sharedProvider.$get();

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
        .otherwise(
        {
            redirectTo: function () {
                return sharedProperties.redirectPage();
            }
        });

    // use HTML5 style links without #
    // it will fallback automatically to # routes on older browsers
    $locationProvider.html5Mode(true);

    console.log("******* Done configuring AngularJS app *******");
}]);

app.run(['$rootScope', 'shared', function ($rootScope, shared) {
    console.log("******* Running AngularJS app *******");

    $rootScope.shared = shared; // getting shared properties into global scope

    // Initialize SignalR in one place http://forums.asp.net/post/5310921.aspx
    shared.hubReady = $.connection.hub.start();

    shared.hubReady.done(function () {
        console.log("Connected to SignalR for realtime updates");
    });
}]);