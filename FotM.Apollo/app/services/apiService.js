function apiFactory($http, $cacheFactory) {

    var lruCache = $cacheFactory('lruCache', { capacity: 50 });

    return {
        loadLeaderboardAsync: function(region, bracket, fotmFilters) {

            var promise = $http({
                method: 'GET',
                url: '/api/' + region + '/' + bracket,
                params: { filters: fotmFilters },
                cache: lruCache
            });

            console.log("cache info:", lruCache.info());

            return promise;
        },

        loadPlayingNowAsync: function(region, bracket) {

            return $http({
                method: 'GET',
                url: '/api/' + region + '/' + bracket+'/now'
            });

        },

        listBlobsAsync: function(container, prefix) {

            return $http({
                method: 'GET',
                url: '/api/listBlobs',
                params: { container: container, prefix: prefix }
            });

        },

        toLocalTime: function(t) {
            var d = new Date(t + " UTC");
            return d.toLocaleDateString() + " " + d.toLocaleTimeString();
        },
    }

};

app.factory('api', ['$http', '$cacheFactory', apiFactory]);
appDebug.factory('api', ['$http', '$cacheFactory', apiFactory]);