app.controller('MainController', function ($scope, $location) {

    console.log("MainController activated");

    //$scope.mainLink = "http://fotm.info";

    $scope.getCurrentLocation = function () { return $location.absUrl(); }

    $scope.getRoot = function() {
        var result = $location.protocol() + "://"+$location.host();

        var port = $location.port();

        return (port) ? (result+":"+port) : result;
    }

});