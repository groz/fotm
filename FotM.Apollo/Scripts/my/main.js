angular.module('fotmApp.controllers', []).controller('fotmController',
    function ($scope) {
        $scope.list = [
            1, 2, 3, 4, 5, 6
        ];
    }
);

angular.module('fotmApp', [
  'fotmApp.controllers'
]);
