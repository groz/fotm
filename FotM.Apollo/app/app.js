var app = angular.module('app', ['ngRoute']);

app.config(function($routeProvider) {

    var regions = ["us", "eu", "kr", "tw", "cn"];
    var brackets = ["2v2", "3v3", "5v5", "rbg"];

    var routeProvider = $routeProvider;

    for (var ir in regions)
        for (var ib in brackets) {

            var region = regions[ir];
            var bracket = brackets[ib];

            // binding closures to loop variables
            var regionProvider = function(r) { return function() { return r; }; }
            var bracketProvider = function(b) { return function() { return b; }; }

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
    
    routeProvider.otherwise({ redirectTo: '/us/3v3' });
});