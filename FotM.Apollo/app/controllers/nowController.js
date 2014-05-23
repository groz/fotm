app.controller('NowController', function (media, api, settings, $scope, $routeParams, $location) {
    console.log("nowController called for", settings);
    console.log("shared settings:", $scope.shared);

    $scope.region = settings.region;
    $scope.bracket = settings.bracket;
    $scope.media = media;

    // notify parent frame of selected region
    $scope.shared.currentRegion = $scope.region;
    $scope.shared.currentBracket = $scope.bracket;
    $scope.shared.now = true;

    api.loadLeaderboardAsync($scope.region, $scope.bracket.text, [])
        .then(function(response) {
            console.log("received data from webapi:", response.data);
            $scope.teams = response.data.Item1;
        });

    $scope.toLocalTime = function(t) {
        var d = new Date(t + " UTC");
        return d.toLocaleDateString() + " " + d.toLocaleTimeString();
    }

    $scope.formatRatingChange = function (n) {
        return (n < 0) ? n.toString() : "+" + n.toString();
    }

});