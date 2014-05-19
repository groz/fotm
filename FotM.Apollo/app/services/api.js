app.factory('api', function($http) {
    
    return {

        loadAsync: function(region, bracket) {

            var promise = $http({
                method: 'GET',
                url: '/api/'+region+'/'+bracket,
                headers: { 'Accept': 'application/json;charset=utf-8' },
                data: '' // Angular ignores headers when this is not set
            });

            return promise;
        }
    }

})