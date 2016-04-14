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
            auth: apiBaseUrl('auth')
        };
        factory.urls = urls;
        // #endregion

        // #region xdinos refactoring
        function httpGet(url) {

            var accesstoken = sessionStorage.getItem('accessToken');
            var authHeaders = {};
            if (accesstoken) {
                authHeaders.Authorization = 'Bearer ' + accesstoken;
            }

            return $http({
                url: url,
                method: "GET",
                headers: authHeaders
            });
        }
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
            //return $http.get(urls.investments + 'getSummaryData');
            return httpGet(urls.investments + 'getSummaryData');
        };
        factory.transferCashToGames = function () {
            return $http.post(urls.investments + 'transferCashToGames');
        }

        factory.getPortfolioData = function () {
            //return $http.get(urls.investments + 'getPortfolioData');
            return httpGet(urls.investments + 'getPortfolioData');
        };
        factory.setPlanSelection = function () {
            return $http.post(urls.investments + 'setPlanSelection');
        }

        factory.getPerformanceData = function () {
            //return $http.get(urls.investments + 'getPerformanceData');
            return httpGet(urls.investments + 'getPerformanceData');
        };
        // #endregion

        // #region Games
        // #endregion

        // #region Guest
        // #endregion

        // #region Auth
        // #endregion

        return factory;
    };
})();