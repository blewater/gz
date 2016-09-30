(function () {
    'use strict';

    APP.config(['$routeProvider', '$locationProvider', 'constants', function($routeProvider, $locationProvider, constants) {
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
    }]);

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

    APP.config(['$wampProvider', 'emConCfg', function ($wampProvider, emConCfg) {        
        $wampProvider.init({
            transports: [
                { 'type': 'websocket', 'url': emConCfg.webSocketApiUrl },
                { 'type': 'longpoll', 'url': emConCfg.fallbackApiUrl }
            ],
            url: emConCfg.websocketApiUrl,
            realm: emConCfg.domainPrefix
        });
    }]);

})();

