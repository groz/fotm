var mainApp = angular.module('mainApp', []);

mainApp.controller('SimpleController', function($scope) {

        $scope.persons = [
            { name: 'Tagir', city: 'Seattle' },
            { name: 'Sergey', city: 'Sydney' },
            { name: 'Maga', city: 'Moscow' },
            { name: 'Albina', city: 'Makhachkala' }
        ];

});
