(function () {
    'use strict';

    APP.factory('api', ['$http', '$rootScope', 'localStorageService', 'constants', serviceFactory]);

    function serviceFactory($http, $rootScope, localStorageService, constants) {
        var factory = {};

        // #region Urls
        function apiBaseUrl(ctrl) {
            return '/api/' + ctrl + 'api/';
        };
        var urls = {
            guest: apiBaseUrl('guest'),
            games: apiBaseUrl('games'),
            investments: apiBaseUrl('investments'),
            auth: apiBaseUrl('auth')
        };
        factory.urls = urls;
        // #endregion

        // #region xdinos refactoring
        //function httpGet(url) {

        //    var authData = localStorageService.get(constants.storageKeys.authData);
        //    var authHeaders = {};
        //    if (authData) {
        //        authHeaders.Authorization = "Bearer " + authData.token;
        //    };

        //    return $http({
        //        url: url,
        //        method: "GET",
        //        headers: authHeaders
        //    });
        //}
        // #endregion

        // #region Common
        factory.call = function (apiFn, resolveFn, options) {
            var defaults = {
                loadingFn: function (flag) { $rootScope.loading = flag; },
                rejectFn: angular.noop,
                errorFn: angular.noop,
                finallyFn: angular.noop
            };
            var ctx = angular.extend(defaults, options);

            ctx.loadingFn(true);
            apiFn().success(function (response) {
                if (response.Ok)
                    resolveFn(response);
                else {
                    console.log(response.Message);
                    //messageService.error(response.Message);
                    ctx.rejectFn(response);
                }
            }).error(function (error) {
                console.log(error);
                //messageService.error(error);
                ctx.errorFn(error);
            }).finally(function () {
                ctx.loadingFn(false);
                ctx.finallyFn();
            });
        };
        // #endregion

        // #region Investments
        factory.getSummaryData = function () {
            return $http.get(urls.investments + 'getSummaryData');
            //return httpGet(urls.investments + 'getSummaryData');
        };
        factory.transferCashToGames = function () {
            return $http.post(urls.investments + 'transferCashToGames');
        }

        factory.getPortfolioData = function () {
            return $http.get(urls.investments + 'getPortfolioData');
            //return httpGet(urls.investments + 'getPortfolioData');
        };
        factory.setPlanSelection = function () {
            return $http.post(urls.investments + 'setPlanSelection');
        }

        factory.getPerformanceData = function () {
            return $http.get(urls.investments + 'getPerformanceData');
            //return httpGet(urls.investments + 'getPerformanceData');
        };
        // #endregion

        // #region Games
        // #endregion

        // #region Guest
        // #endregion

        // #region Auth
        factory.login = function (usernameOrEmail, password) {
            var data = "grant_type=password" +
                       "&username=" + usernameOrEmail +
                       "&password=" + password;
            return $http({
                url: "/TOKEN",
                method: "POST",
                data: data,
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            });
        }

        factory.logout = function () {
            localStorageService.remove(constants.storageKeys.authData);
            // TODO
            //var templates = $filter('toArray')(constants.templates);
            //for (var i = 0; i < templates.length; i++)
            //    $templateCache.remove(templates[i]);
            //message.clear();
        }

        factory.register = function (parameters) {
            return $http.post('/api/Account/Register', parameters);
        }

        factory.forgotPassword = function (email) {
            return $http({
                url: 'api/Account/ForgotPassword',
                method: 'POST',
                data: { Email: email }
            });
        }

        factory.resetPassword = function (parameters) {
            return $http({
                url: 'api/Account/ResetPassword',
                method: 'POST',
                data: {
                    Email: email,
                    Password: $scope.model.password,
                    ConfirmPassword: $scope.model.password,
                    Code: $scope.model.code
                }
            });
        }
        // #endregion

        return factory;
    };
})();