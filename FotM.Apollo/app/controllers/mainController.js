app.controller('MainController', ['$scope', '$location', '$window', '$rootScope', '$cookies',
    function ($scope, $location, $window, $rootScope, $cookies) {

    console.log("MainController activated");

    console.log("cookies:", $cookies, "shared:", $scope.shared);

    $scope.getCurrentLocation = function () { return $location.absUrl(); }

    $scope.getRoot = function() {
        var result = $location.protocol() + "://"+$location.host();

        var port = $location.port();

        return (port) ? (result+":"+port) : result;
    }

    var history = ["outside location"];

    $rootScope.$on('$viewContentLoaded', function () {
        var virtualPage = $location.path();
        var previousPage = history[history.length-1];

        console.log("moved from", previousPage, "to", virtualPage);

        history.push(virtualPage);
        $cookies.lastPage = virtualPage;
        
        if (virtualPage != previousPage) {
            console.log("logging pageview to analytics:", virtualPage);
            $window.ga('send', 'pageview', virtualPage);
        }
    });

    $scope.logRedirect = function (target) {
        // logging clicks on social buttons in /about
        $window.ga('send', 'pageview', target);
    }

}]);