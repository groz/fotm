app.factory('api', function ($http, $cacheFactory) {

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

            var promise = $http({
                method: 'GET',
                url: '/api/' + region + '/' + bracket+'/now'
            });

            return promise;
        }
    }

});