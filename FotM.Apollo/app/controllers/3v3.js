app.controller('ThreesController', function(media, api, $scope) {

    $scope.media = media;

    $scope.greeting = "3v3";
    $scope.teams = {}
    

    api.loadAsync().then(function (response) {
        console.log("received 3v3 data: ", response.data);
        $scope.teams = response.data;
    });

});

