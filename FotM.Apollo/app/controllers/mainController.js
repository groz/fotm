app.controller('MainController', function ($scope, $location, $window, $rootScope) {

    console.log("MainController activated");

    $scope.getCurrentLocation = function () { return $location.absUrl(); }

    $scope.getRoot = function() {
        var result = $location.protocol() + "://"+$location.host();

        var port = $location.port();

        return (port) ? (result+":"+port) : result;
    }

    $rootScope.$on('$routeChangeSuccess', function () {
        var virtualPage = $location.path();
        console.log("logging pageview to analytics:", virtualPage);
        $window.ga('send', 'pageview', virtualPage);
    });

});