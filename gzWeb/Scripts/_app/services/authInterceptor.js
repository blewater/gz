(function () {
    'use strict';

    APP.factory('authInterceptor', ['$q', '$location', 'constants', authInterceptor]);

    function authInterceptor($q, $location, constants) {
        var factory = {};

        factory.request = function (config) {
            config.headers = config.headers || { 'Content-type': 'application/json' };
            return config;
        }

        factory.responseError = function (rejection) {
            if (rejection.status === 401) {
                $location.path(constants.routes.login.path);
            }
            return $q.reject(rejection);
        }
        return factory;
    };
})();