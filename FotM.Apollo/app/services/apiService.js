app.factory('api', function($http) {
    
    return {

        loadLeaderboardAsync: function(region, bracket, fotmFilters) {

            var promise = $http({
                method: 'GET',
                url: '/api/'+region+'/'+bracket,
                params: { filters: fotmFilters }
            });

            return promise;
        }
    }

})