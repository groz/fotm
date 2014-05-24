app.controller('MainController', function ($scope, $location) {

    console.log("MainController activated");

    $scope.getCurrentLocation = function () { return $location.absUrl(); }

    $scope.getRoot = function() {
        var result = $location.protocol() + "://"+$location.host();

        var port = $location.port();

        return (port) ? (result+":"+port) : result;
    }

    $scope.title = "Flavor of the Month team ratings for World of Warcraft";

    $scope.description = "Website that shows currently popular team combinations for World of Warcraft arena. It allows for filtering of setups to help finding best one for your team composition and brings the feel of old single-team arenas back.";

    $scope.previewUrl = function() { return $scope.getRoot() + "/preview.png"; }

});