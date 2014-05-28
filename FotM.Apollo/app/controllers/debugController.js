appDebug.controller('DebugController', ['$scope', 'api', function ($scope, api) {

    $scope.container = "snapshots";
    $scope.prefix = "US/3v3";
    $scope.toLocalTime = api.toLocalTime;

    $scope.listBlobs = function () {
        api.listBlobsAsync($scope.container, $scope.prefix).then(function (response) {
            $scope.blobList = response.data;
        });
    };

    $scope.showBlob = function(blobUri) {
        api.showBlobAsync(blobUri).then(function (response) {
            $scope.selectedBlobText = response.data;
        });
    }

    $scope.formatSize = function(size) {
        return Math.floor(size/1024) + " KB";
    }

}]);