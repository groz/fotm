app.controller('ApiController', function(media, api, region, bracket, $scope, $routeParams) {

    var filter = $routeParams.filter;

    console.log("apiController called for", region, bracket, filter);

    $scope.region = region;
    $scope.bracket = bracket;
    $scope.media = media;

    $scope.teams = {}
    
    api.loadAsync($scope.region, $scope.bracket).then(function (response) {
        console.log("received data:", response.data);
        $scope.teams = response.data;
    });

});