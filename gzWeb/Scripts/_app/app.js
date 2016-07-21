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
    ]);

    app.run([
        '$rootScope', '$location', '$window', '$route', '$timeout', 'screenSize', 'localStorageService', 'constants', 'auth', 'chat', 'helpers',
        function ($rootScope, $location, $window, $route, $timeout, screenSize, localStorageService, constants, auth, chat, helpers) {
            $rootScope.loading = true;
            $rootScope.initialized = false;
            localStorageService.set(constants.storageKeys.randomSuffix, Math.random());

            angular.element(document).ready(function () {
                $rootScope.mobile = helpers.ui.isMobile();
            });

            auth.init();

            $rootScope.$on(constants.events.ON_INIT, function () {
                function setRouteData(route) {
                    var category = route.category;
                    if (angular.isDefined(category))
                        $rootScope.routeData = {
                            category: category,
                            wandering: category === constants.categories.wandering,
                            gaming: category === constants.categories.gaming,
                            investing: category === constants.categories.investing
                        }
                }

                var currentRoute = $route.current.$$route;
                if (!auth.authorize(currentRoute.roles))
                    $location.path(constants.routes.home.path);
                else
                    setRouteData(currentRoute);

                $rootScope.$on('$routeChangeStart', function (event, next, current) {
                    $rootScope.loading = true;
                    $rootScope.mobileMenuExpanded = false;
                    if (next && !auth.authorize(next.roles))
                        $location.path(constants.routes.home.path);
                });

                $rootScope.$on('$routeChangeSuccess', function () {
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

                $rootScope.xs = screenSize.on('xs', function (match) { $rootScope.xs = match; });
                $rootScope.sm = screenSize.on('sm', function (match) { $rootScope.sm = match; });
                $rootScope.md = screenSize.on('md', function (match) { $rootScope.md = match; });
                $rootScope.lg = screenSize.on('lg', function (match) { $rootScope.lg = match; });
                $rootScope.size = screenSize.get();
                screenSize.on('xs,sm,md,lg', function () {
                    $rootScope.size = screenSize.get();
                });

                $rootScope.scrolled = false;
                $rootScope.scrollOffset = 0;
                angular.element($window).bind("scroll", function () {
                    $rootScope.scrolled = this.pageYOffset > 0;
                    $rootScope.scrollOffset = this.pageYOffset;
                    $rootScope.$apply();
                });

                $timeout(function () {
                    var $preloader = angular.element(document.querySelector('#preloader'));
                    $preloader.addClass('die');
                    $timeout(function () { $preloader.remove(); }, 2000);
                }, 1000);

                $rootScope.loading = false;
                $rootScope.initialized = true;
                chat.show();
            });
        }
    ]);

    //angular.element(document).ready(function () {
    //    angular.bootstrap(document, [id]);
    //});

    return app;
})(APP);