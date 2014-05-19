app.controller('ThreesController', function(media, api, $scope) {

    $scope.region = "us";
    $scope.bracket = "3v3";
    $scope.media = media;

    $scope.teams = {}
    
    api.loadAsync($scope.region, $scope.bracket).then(function (response) {
        console.log("received data:", response.data);
        $scope.teams = response.data;
    });

});

