app.directive('userAvatar', ['media', function (media) {

    return {
        restrict: 'E',
        scope: {
            vm: "="
        },
        link: function (scope, element, attrs) {
            scope.media = media;
        },
        templateUrl: 'app/templates/userAvatar.html'
    };

}]);
