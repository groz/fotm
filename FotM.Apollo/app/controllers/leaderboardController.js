app.controller('LeaderboardController', ['filterFactory', 'media', 'api', 'settings', '$scope', '$routeParams', '$location',
    function (filterFactory, media, api, settings, $scope, $routeParams, $location) {
    var inputFilters = $routeParams.filter;

    if (typeof (inputFilters) == "string")
        inputFilters = [inputFilters];

    console.log("leaderboardController called for", settings);

    console.log("shared:", $scope.shared);

    $scope.region = settings.region;
    $scope.bracket = settings.bracket;
    $scope.media = media;

    // notify parent frame of selected region
    $scope.shared.currentRegion = $scope.region;
    $scope.shared.currentBracket = $scope.bracket;
    $scope.shared.now = false;
    $scope.shared.lastSnapshotLocation = "";

    $scope.teams = {}
    $scope.setups = {}

    $scope.ordering = $routeParams.ordering;

    $scope.setupNumber = function(setup) {
        if ($scope.ordering == "winRatio") {
            return api.toPercent(setup.winRatio);
        }
        return api.toPercent(setup.popularity);
    };

    $scope.fotmFilters = filterFactory.createFilters($scope.bracket.size, inputFilters);
    

    api.loadLeaderboardAsync($scope.region, $scope.bracket.text, $scope.fotmFilters, $scope.ordering)
        .then(function(response) {
            console.log("received data from webapi:", response.data);
            $scope.teams = response.data.Item1;
            $scope.setups = response.data.Item2;
            console.log("Last snapshot location:", response.data.Item3);
        });

    console.log("loadLeaderBoard request queued.");

    $scope.getSpecForFilter = function (fotmFilter) {
        return media.getSpecInfo(fotmFilter.specId);
    };

    $scope.getAllSpecsFor = function (fotmFilter) {
        var className = fotmFilter.className;
        var specIds = media.getSpecsFor(className);

        return specIds.reduce(function (obj, id) {
            obj[id] = media.getSpecInfo(id);
            return obj;
        }, {});
    };

    $scope.redirectToFilter = function() {
        console.log("redirectToFilter");

        var nonEmpty = false;

        var filterStrings = $scope.fotmFilters.reduce(function(arr, f) {
            var filterString = f.toString();

            if (filterString != null)
                nonEmpty = true;

            console.log(f, "reduced to", filterString);

            arr.push(filterString);
            return arr;
        }, []);

        if (nonEmpty)
            $location.search( { filter: filterStrings } );
        else
            $location.search("");
    };

    $scope.redirectToSetupFilter = function (setup) {
        for (var i = 0; i < setup.specs.length; ++i) {
            $scope.fotmFilters[i].className = media.classText(setup.specs[i]);
            $scope.fotmFilters[i].specId = media.getSpecId(setup.specs[i]);
        }
        $scope.redirectToFilter();
    };

}]);
