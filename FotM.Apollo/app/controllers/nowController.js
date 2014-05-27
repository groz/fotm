app.controller('NowController', ['media', 'api', 'settings', '$scope', function (media, api, settings, $scope) {
    console.log("nowController called for", settings);
    console.log("shared settings:", $scope.shared);

    $scope.region = settings.region;
    $scope.bracket = settings.bracket;
    $scope.media = media;

    // notify parent frame of selected region
    $scope.shared.currentRegion = $scope.region;
    $scope.shared.currentBracket = $scope.bracket;
    $scope.shared.now = true;

    api.loadPlayingNowAsync($scope.region, $scope.bracket.text, [])
        .then(function(response) {
            console.log("received data from now webapi:", response.data);
            $scope.teams = response.data;
            $scope.empty = $scope.teams.length == 0;
        });

    $scope.toLocalTime = function(t) {
        var d = new Date(t + " UTC");
        return d.toLocaleDateString() + " " + d.toLocaleTimeString();
    }

    $scope.formatRatingChange = function (n) {
        return (n < 0) ? n.toString() : "+" + n.toString();
    }

}]);