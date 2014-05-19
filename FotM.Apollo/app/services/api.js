app.factory('api', function($http) {
    
    var apiService = {
        loadAsync: function() {
            var promise = $http.get('/api2/values');
            return promise;
        }
    }

    return apiService;

})