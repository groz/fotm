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

    $scope.teams = [];

    // TODO: refactor paging functionality into separate service?
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

    $scope.nextPage = function () {
        if ($scope.currentPage < $scope.nPages - 1)
            $scope.currentPage++;
    }

    $scope.previousPage = function () {
        if ($scope.currentPage > 0)
            $scope.currentPage--;
    }

    function fetchData() {
        api.loadPlayingNowAsync($scope.region, $scope.bracket.text)
            .then(function(response) {
                console.log("received data from now webapi, length:", response.data.length);
                $scope.teams = response.data;

                var nTotalTeams = $scope.teams.length;
                $scope.empty = nTotalTeams == 0;
                $scope.nPages = Math.ceil(nTotalTeams / nTeamsOnPage);
                console.log("total teams: ", nTotalTeams, ", total pages: ", $scope.nPages);
            });
    }

    fetchData();

    $scope.toLocalTime = api.toLocalTime;

    $scope.formatRatingChange = function (n) {
        return (n < 0) ? n.toString() : "+" + n.toString();
    }

    // subscribe to realtime updates notification
    var notifier = $.connection.playingNowHub;

    notifier.client.updateReady = function(region, bracket, updateTime) {
        console.log("NOTIFICATION RECEIVED: update ready for", region, bracket, api.toLocalTime(updateTime));
        if ((region.toUpperCase() === $scope.region.toUpperCase())
            &&
            (bracket.toUpperCase() === $scope.bracket.text.toUpperCase())) {

            console.log("This is our update, fetching data...");
            fetchData();
        }
    }

}]);