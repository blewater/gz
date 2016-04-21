(function () {
    'use strict';

    APP.factory('authInterceptor', ['$q', '$location', 'localStorageService', 'constants', 'route', authInterceptor]);

    function authInterceptor($q, $location, localStorageService, constants, route) {
        var factory = {};

        factory.request = function (config) {
            config.headers = config.headers || { 'Content-type': 'application/json' };
            var authData = localStorageService.get(constants.storageKeys.authData);
            if (authData)
                config.headers.Authorization = "Bearer " + authData.token;
            return config;
        }

        factory.responseError = function (rejection) {
            if (rejection.status == 404) {
                // TODO: create 404 page
                //$location.path(route.getPath(constants.routeKeys.notFound));
                return true;
            }
            else if (rejection.status == 401) {
                localStorageService.remove(constants.storageKeys.authData);
                $location.path(route.getPath(constants.routeKeys.home));
                return true;
            }
            else
                return $q.reject(rejection);
            //if (rejection.status === 401) {
            //    $location.path(route.getPath(constants.routeKeys.login));
            //}
            //return $q.reject(rejection);
        }
        return factory;
    };
})();