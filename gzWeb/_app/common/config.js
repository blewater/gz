(function () {
    'use strict';

    APP.config(['$routeProvider', '$locationProvider', 'constants', 'insightsProvider', 'appInsightsCfg', function ($routeProvider, $locationProvider, constants, insightsProvider, appInsightsCfg) {
        var routes = constants.routes.all;
        for (var i = 0; i < routes.length; i++)
            $routeProvider.when(routes[i].path, {
                controller: routes[i].ctrl,
                templateUrl: routes[i].tpl,
                title: routes[i].title,
                reloadOnSearch: routes[i].reloadOnSearch,
                category: routes[i].category,
                roles: routes[i].roles
            });
        $routeProvider.otherwise({ redirectTo: '/' });

        $locationProvider.html5Mode(constants.html5Mode).hashPrefix();
        insightsProvider.start(appInsightsCfg.key);
    }]);

    APP.config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('authInterceptor');
        //$httpProvider.defaults.headers.common['Cache-Control'] = 'no-cache, no-store, must-revalidate';
        //$httpProvider.defaults.headers.common['Pragma'] = 'no-cache';
        //$httpProvider.defaults.headers.common['Expires'] = '0';
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

    APP.config(['$wampProvider', 'emConCfg', function ($wampProvider, emConCfg) {        
        $wampProvider.init({
            transports: [
                { 'type': 'websocket', 'url': emConCfg.webSocketApiUrl, 'max_retries': 3 },
                { 'type': 'longpoll', 'url': emConCfg.fallbackApiUrl }
            ],
            url: emConCfg.websocketApiUrl,
            realm: emConCfg.domainPrefix
        });
    }]);

})();

