app.factory('api', function($http) {
    
    var apiService = {
        loadAsync: function() {

            var promise = $http({
                method: 'GET',
                url: '/api2/values',
                headers: { 'Accept': 'application/json;charset=utf-8' },
                data: '' // Angular ignores headers when this is not set
            });

            return promise;
        }
    }

    return apiService;

})