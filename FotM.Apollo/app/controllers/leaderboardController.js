app.controller('LeaderboardController', ['filterFactory', 'media', 'api', 'settings', '$scope', '$routeParams', '$location', function (filterFactory, media, api, settings, $scope, $routeParams, $location) {
    var inputFilters = $routeParams.filter;

    if (typeof (inputFilters) == "string")
        inputFilters = [inputFilters];

    console.log("leaderboardController called for", settings);

    console.log("shared:", $scope.shared);

    $scope.region = settings.region;
    $scope.bracket = settings.bracket;
    $scope.media = media;

    $scope.nPages = 1;
    $scope.currentPage = 0;
    var nTeamsOnPage = 10;

    $scope.teamsOnPage = function () {
        var result = [];

        var first = $scope.currentPage * nTeamsOnPage;
        var last = Math.min(first + nTeamsOnPage, $scope.teams.length);

        for (var i = first; i < last; ++i)
            result.push($scope.teams[i]);

        return result;
    }

    var pageId = $scope.region + ' ' + $scope.bracket.text + ' leaderboard';

    $scope.nextPage = function () {
        if ($scope.currentPage < $scope.nPages - 1) {
            $scope.currentPage++;
            ga('send', 'event', 'change page', 'next', pageId, $scope.currentPage);
        }
    }

    $scope.previousPage = function () {
        if ($scope.currentPage > 0) {
            $scope.currentPage--;
            ga('send', 'event', 'change page', 'previous', pageId, $scope.currentPage);
        }
    }

    $scope.armoryLookup = function (player) {
        var playerId = player.name + ' ' + player.realm.realmName;
        ga('send', 'event', 'armory lookup', pageId, playerId);
    }

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
            console.log("received data from webapi, teams:", response.data.Item1.length);
            $scope.teams = response.data.Item1;
            $scope.setups = response.data.Item2;
            console.log("Last snapshot location:", response.data.Item3);

            var nTotalTeams = $scope.teams.length;
            $scope.nPages = Math.ceil(nTotalTeams / nTeamsOnPage);
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

            if (filterString != null) {
                nonEmpty = true;
                ga('send', 'event', 'filter', filterString); // log each filter separately
            }

            console.log(f, "reduced to", filterString);

            arr.push(filterString);
            return arr;
        }, []);

        // log composite filtering if several filters are applied
        if (filterStrings.length > 1 && nonEmpty) {
            ga('send', 'event', 'composite filter', filterStrings.join());
        }

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
