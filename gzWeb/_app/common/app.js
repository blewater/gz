var APP = (function () {
    'use strict';

    var id = 'GZ';

    var app = angular.module(id, [
        // Angular modules 
        'ngRoute',
        , 'ngResource'
        , 'ngAnimate'
        , 'ngSanitize'
        , 'ngCookies'
        , 'ngTouch'

        // Custom modules 
        , 'customDirectives'
        , 'customFilters'
        //, 'styleInjector'

        // 3rd Party Modules
        , 'ui.bootstrap'
        , 'angular.filter'
        , 'angularSpinner'
        , 'angularMoment'
        , 'matchMedia'
        , 'LocalStorageModule'
        , 'angularSpinner'
        , 'FBAngular'
        , 'vxWamp'
        , 'ngAutocomplete'
        , 'vcRecaptcha'
        , 'isoCurrency'
        , 'logToServer'
        , 'ui.bootstrap.datetimepicker'
    ]);

    app.run([
        '$rootScope', '$location', '$window', '$route', '$timeout', 'screenSize', 'localStorageService', 'constants', 'auth', 'chat', 'helpers', 'nav',
        function ($rootScope, $location, $window, $route, $timeout, screenSize, localStorageService, constants, auth, chat, helpers, nav) {

            var defaultBeforeSend = function(xhr, json) {
                var authData = localStorageService.get(constants.storageKeys.authData);
                if (authData)
                    xhr.setRequestHeader('Authorization', 'Bearer ' + authData.token);
            };
            JL.setOptions({ "defaultBeforeSend": defaultBeforeSend });

            $rootScope.loading = true;
            $rootScope.initialized = false;
            $rootScope.redirected = false;
            localStorageService.set(constants.storageKeys.randomSuffix, Math.random());

            angular.element(document).ready(function () {
                $rootScope.mobile = helpers.ui.isMobile();
            });

            auth.init();

            $rootScope.$on(constants.events.ON_INIT, function () {
                function hidePreloader() {
                    var $preloader = angular.element(document.querySelector('#preloader'));
                    $preloader.addClass('die');
                    $timeout(function () { $preloader.remove(); }, 1000);
                };

                function setRouteData(route) {
                    var category = route.category;
                    if (angular.isDefined(category)) {
                        $rootScope.routeData = {
                            category: category,
                            wandering: category === constants.categories.wandering,
                            gaming: category === constants.categories.gaming,
                            investing: category === constants.categories.investing
                        }
                    }
                }

                $rootScope.$on('$routeChangeStart', function (event, next, current) {
                    $rootScope.loading = true;
                    $rootScope.mobileMenuExpanded = false;
                    if (next && !auth.authorize(next.roles))
                        $location.path(constants.routes.home.path);
                });

                function onRouteChangeSuccess() {
                    $timeout(function () {
                        $rootScope.loading = false;
                        $rootScope.documentTitle = constants.title;
                        if ($route.current.$$route) {
                            if ($route.current.$$route.title) {
                                $rootScope.title = $route.current.$$route.title;
                                $rootScope.documentTitle += " - " + $rootScope.title;
                            }
                            setRouteData($route.current.$$route);
                        }
                    });
                };
                //onRouteChangeSuccess();
                $rootScope.$on('$routeChangeSuccess', onRouteChangeSuccess);

                helpers.ui.watchScreenSize($rootScope);
                helpers.ui.watchWindowScroll($rootScope);
                chat.show();
                $rootScope.loading = false;
                $rootScope.initialized = true;
                hidePreloader();

                if (!auth.authorize($route.current.$$route.roles)) {
                    nav.setRequestUrl($location.$$path);
                    $location.path(constants.routes.home.path);
                    $rootScope.redirected = true;
                }

                $rootScope.$broadcast(constants.events.ON_AFTER_INIT);
            });
        }
    ]);

    //angular.element(document).ready(function () {
    //    angular.bootstrap(document, [id]);
    //});

    return app;
})(APP);