var mainApp = angular.module('mainApp', ['ngRoute']);

mainApp.controller('TwosController', function($scope) {

    $scope.greeting = "2v2";

});

mainApp.controller('ThreesController', function($scope) {

    $scope.greeting = "3v3";

    $scope.persons = [
        { name: 'Tagir', city: 'Seattle' },
        { name: 'Sergey', city: 'Sydney' },
        { name: 'Maga', city: 'Moscow' },
        { name: 'Albina', city: 'Makhachkala' }
    ];

});

mainApp.controller('FivesController', function ($scope) {

    $scope.greeting = "5v5";

});

mainApp.controller('RbgController', function ($scope) {

    $scope.greeting = "rbg";

});

mainApp.config(function($routeProvider) {

    $routeProvider
        .when('/2v2',
        {
            controller: 'TwosController',
            templateUrl: 'Content/Partials/View2.html'
        })
        .when('/3v3',
        {
            controller: 'ThreesController',
            templateUrl: 'Content/Partials/View3.html'
        })
        .when('/5v5',
        {
            controller: 'FivesController',
            templateUrl: 'Content/Partials/View5.html'
        })
        .when('/rbg',
        {
            controller: 'RbgController',
            templateUrl: 'Content/Partials/ViewRbg.html'
        })
        .otherwise({ redirectTo: '/3v3' });

});
