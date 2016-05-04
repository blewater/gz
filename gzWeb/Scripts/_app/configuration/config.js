(function () {
    'use strict';

    APP.config(['$routeProvider', '$locationProvider', 'constants', function($routeProvider, $locationProvider, constants) {
        for (var i = 0; i < constants.routes.length; i++)
            $routeProvider.when(constants.routes[i].path, {
                controller: constants.routes[i].ctrl,
                templateUrl: constants.routes[i].tpl,
                title: constants.routes[i].title,
                reloadOnSearch: constants.routes[i].reloadOnSearch || true
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

    APP.config(['$wampProvider', 'constants', function ($wampProvider, constants) {
        
        $wampProvider.init({
            transports: [
                { 'type': 'websocket', 'url': constants.webeocketApiUrl },
                { 'type': 'longpoll', 'url': constants.fallbackApiUrl }
            ],
            url: constants.websocketApiUrl,
            realm: constants.domainPrefix
        });
    }]);

})();

