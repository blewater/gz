﻿(function () {
    'use strict';

    APP.factory('authInterceptor', ['$q', '$location', 'localStorageService', 'constants', authInterceptor]);

    function authInterceptor($q, $location, localStorageService, constants) {
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
                //$location.path(constants.routes.notFound.path);
                return true;
            }
            else if (rejection.status == 401) {
                //localStorageService.remove(constants.storageKeys.authData);
                $location.path(constants.routes.home.path);
                return true;
            }
            else
                return $q.reject(rejection);
        }
        return factory;
    };
})();