(function () {
    'use strict';

    APP.factory('api', ['$http', '$rootScope', serviceFactory]);

    function serviceFactory($http, $rootScope) {
        var factory = {};

        // #region Urls
        function apiBaseUrl(ctrl) {
            return '/api/' + ctrl + 'api/';
        };
        var urls = {
            guest: apiBaseUrl('guest'),
            games: apiBaseUrl('games'),
            investments: apiBaseUrl('investments'),
            //account: apiBaseUrl('account')
        };
        factory.urls = urls;
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
                    //console.log(response.Message);
                    //message.error(response.Message);
                    ctx.rejectFn(response);
                }
            }).error(function (error) {
                //console.log(error);
                //message.error(error);
                ctx.errorFn(error);
            }).finally(function () {
                ctx.loadingFn(false);
                ctx.finallyFn();
            });
        };
        // #endregion

        // #region Account
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

        factory.register = function (parameters) {
            return $http.post('/api/Account/Register', parameters);
        }

        factory.revokeRegistration = function () {
            return $http.post('/api/Account/RevokeRegistration');
        }

        factory.finalizeRegistration = function (userId) {
            return $http.post('/api/Account/FinalizeRegistration?userId=' + userId);
        }

        factory.forgotPassword = function (email) {
            return $http.post('/api/Account/ForgotPassword', { Email: email });
        }

        factory.resetPassword = function (parameters) {
            return $http.post('/api/Account/ResetPassword', parameters);
        }

        factory.changePassword = function (parameters) {
            return $http.post('/api/Account/ChangePassword', parameters);
        }

        factory.getDeploymentInfo = function () {
            return $http.get('/api/Account/GetDeploymentInfo');
        }
        factory.cacheUserData = function () {
            return $http.get('/api/Account/CacheUserData');
        }
        // #endregion

        // #region Investments
        factory.getSummaryData = function () {
            return $http.get(urls.investments + 'getSummaryData');
        };
        factory.getVintagesWithSellingValues = function (vintages) {
            return $http.post(urls.investments + 'getVintagesWithSellingValues', vintages);
        };
        factory.withdrawVintages = function (vintages) {
            return $http.post(urls.investments + 'withdrawVintages', vintages);
        }

        factory.getPortfolioData = function () {
            return $http.get(urls.investments + 'getPortfolioData');
        };
        factory.setPlanSelection = function (plan) {
            return $http.post(urls.investments + 'setPlanSelection', plan);
        }

        factory.getPerformanceData = function () {
            return $http.get(urls.investments + 'getPerformanceData');
        };
        // #endregion

        // #region Games
        // #endregion

        // #region Guest
        // #endregion

        return factory;
    };
})();