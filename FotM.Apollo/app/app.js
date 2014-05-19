var app = angular.module('app', ['ngRoute']);

app.config(function($routeProvider) {

    $routeProvider
        .when('/2v2',
        {
            controller: 'TwosController',
            templateUrl: 'app/templates/2v2.html'
        })
        .when('/3v3',
        {
            controller: 'ThreesController',
            templateUrl: 'app/templates/3v3.html'
        })
        .when('/5v5',
        {
            controller: 'FivesController',
            templateUrl: 'app/templates/5v5.html'
        })
        .when('/rbg',
        {
            controller: 'RbgController',
            templateUrl: 'app/templates/rbg.html'
        })
        .otherwise({ redirectTo: '/3v3' });

});
