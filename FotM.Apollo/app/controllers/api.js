app.controller('ApiController', function(media, api, region, bracket, $scope, $routeParams) {

    var filter = $routeParams.filter;

    console.log("apiController called for", region, bracket, filter);

    $scope.region = region;
    $scope.bracket = bracket;
    $scope.media = media;

    $scope.teams = {}

    $scope.fotmFilters = [];

    for (var i = 0; i < bracket.size; ++i) {
        $scope.fotmFilters.push({
            classFilter: null,
            specFilter: null
        });
    }
    
    api.loadAsync($scope.region, $scope.bracket.text).then(function (response) {
        console.log("received data:", response.data);
        $scope.teams = response.data;
    });

    $scope.getSpecsFor = function (idx) {
        var f = $scope.fotmFilters[idx];
        console.log(f);

        if (f.classFilter !== null) {
            var specIds = media.classes[f.classFilter].specs;

            var specs = {};

            for (var is in specIds) {
                var specId = specIds[is];
                specs[specId] = media.specs[specId];
            }

            console.log(specs);
            return specs;
        }
        return [];
    };

});