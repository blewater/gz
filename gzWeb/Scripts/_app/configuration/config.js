(function () {
    'use strict';

    APP.config(['$routeProvider', '$locationProvider', 'constants', config]);
    function config($routeProvider, $locationProvider, constants) {

        for (var i = 0; i < constants.routes.length; i++)
            $routeProvider.when(constants.routes[i].path, {
                controller: constants.routes[i].ctrl,
                templateUrl: constants.routes[i].tpl,
                title: constants.routes[i].title
            });
        $routeProvider.otherwise({ redirectTo: '/' });

        $locationProvider.html5Mode(constants.html5Mode).hashPrefix();
    }

    APP.config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('authInterceptor');
    }]);

    APP.config(['$provide', 'constants', function ($provide, constants) {
        $provide.decorator('$templateRequest', ['$delegate', function ($delegate) {
            var silentProvider = function (template, ignoreRequestError) {
                var prefix = constants.area;
                var ignore = template.toLowerCase().indexOf(prefix.toLowerCase()) == 0 ? true : ignoreRequestError;
                return $delegate(template, ignore);
            }
            return silentProvider;
        }]);
    }]);
})();

