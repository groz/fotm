function apiFactory($http, $cacheFactory) {

    var lruCache = $cacheFactory('lruCache', { capacity: 50 });

    return {
        loadLeaderboardAsync: function(region, bracket, fotmFilters, ordering) {

            var promise = $http({
                method: 'GET',
                url: '/api/' + region + '/' + bracket,
                params: { filters: fotmFilters, ordering: ordering },
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

        showBlobAsync: function(blobUri) {
            return $http({
                method: 'GET',
                url: 'api/showBlob',
                params: { blobUri: blobUri }
            });
        },

        toLocalTime: function(t) {
            var d = new Date(t + " UTC");
            return d.toLocaleDateString() + " " + d.toLocaleTimeString();
        },

        toPercent: function(v) {
            return (v * 100).toFixed(1) + "%";
        },
    }

};

app.factory('api', ['$http', '$cacheFactory', apiFactory]);
appDebug.factory('api', ['$http', '$cacheFactory', apiFactory]);