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

    function fetchData() {
        api.loadPlayingNowAsync($scope.region, $scope.bracket.text)
            .then(function(response) {
                console.log("received data from now webapi, length:", response.data.length);
                $scope.teams = response.data;
                $scope.empty = $scope.teams.length == 0;
            });
    }

    fetchData();

    $scope.toLocalTime = api.toLocalTime;

    $scope.formatRatingChange = function (n) {
        return (n < 0) ? n.toString() : "+" + n.toString();
    }

    // subscribe to realtime updates notification
    var notifier = $.connection.playingNowHub;

    notifier.client.updateReady = function(region, bracket) {
        console.log("NOTIFICATION RECEIVED: update ready for", region, bracket);
        if ((region.toUpperCase() === $scope.region.toUpperCase())
            &&
            (bracket.toUpperCase() === $scope.bracket.text.toUpperCase())) {

            console.log("This is our update, fetching data...");
            fetchData();
        }
    }

    $.connection.hub.start().done(function() {
        console.log("SUBSCRIBED TO REALTIME NOTIFICATIONS");
    });

    $scope.$on("$destroy", function () {
        console.log("Exiting NowController. Unsubscribing from notifications.");
        $.connection.hub.stop();
    });

}]);