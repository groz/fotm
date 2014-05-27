appDebug.controller('DebugController', ['$scope', 'api', function ($scope, api) {

    $scope.container = "snapshots";
    $scope.prefix = "US/3v3";
    $scope.toLocalTime = api.toLocalTime;

    $scope.listBlobs = function () {
        api.listBlobsAsync($scope.container, $scope.prefix).then(function (response) {
            $scope.blobList = response.data;
        });
    };

}]);