(function () {
    'use strict';

    APP.factory('authInterceptor', ['$q', '$location', 'constants', 'route', authInterceptor]);

    function authInterceptor($q, $location, constants, route) {
        var factory = {};

        factory.request = function (config) {
            config.headers = config.headers || { 'Content-type': 'application/json' };
            return config;
        }

        factory.responseError = function (rejection) {
            if (rejection.status === 401) {
                $location.path(route.getPath(constants.routeKeys.login));
            }
            return $q.reject(rejection);
        }
        return factory;
    };
})();