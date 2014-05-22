app.controller('ApiController', function (filterFactory, media, api, region, bracket, $scope, $routeParams, $location) {
    console.log(filterFactory);
    var inputFilters = $routeParams.filter;

    if (typeof (inputFilters) == "string")
        inputFilters = [inputFilters];

    console.log("apiController called for", region, bracket, inputFilters);

    $scope.region = region;
    $scope.bracket = bracket;
    $scope.media = media;

    $scope.teams = {}
    $scope.setups = {}

    $scope.fotmFilters = [];

    for (var i = 0; i < bracket.size; ++i) {
        var ithFilter = (inputFilters && inputFilters[i])
                        ? filterFactory.createFromString(inputFilters[i])
                        : filterFactory.create(null, null);

        console.log("applying filter", ithFilter);
        $scope.fotmFilters.push(ithFilter);
    }

    api.loadAsync($scope.region, $scope.bracket.text, $scope.fotmFilters).then(function (response) {
        console.log("received data:", response.data);
        $scope.teams = response.data.Item1;
        $scope.setups = response.data.Item2;
    });

    $scope.getSpecsFor = function (idx) {
        var specIds = media.getSpecsFor($scope.fotmFilters[idx].className);

        return specIds.reduce(function (obj, id) {
            obj[id] = media.getSpecInfo(id);
            return obj;
        }, {});
    };

    $scope.redirectToFilter = function() {
        console.log("redirectToFilter");

        var filterStrings = $scope.fotmFilters.reduce(function(arr, f) {
            var filterString = f.toString();
            console.log(f, "reduced to", filterString);

            arr.push(filterString);
            return arr;
        }, []);

        $location.search({
            filter: filterStrings
        });
    };

    $scope.getSpecForFilter = function (filterIndex) {
        var filter = $scope.fotmFilters[filterIndex];
        return media.getSpecInfo(filter.specId);
    };

});