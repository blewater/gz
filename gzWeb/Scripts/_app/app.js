var APP = (function () {
    'use strict';

    var id = 'GZ';
    var app = angular.module(id, [
        // Angular modules 
        'ngRoute',
        'ngResource',
        'ngAnimate',
        'ngSanitize',
        'ngCookies'

        // Custom modules 
        //, 'customDirectives'
        //, 'customFilters'
        //, 'styleInjector'

        // 3rd Party Modules
        , 'ui.bootstrap'
        , 'angular.filter'
        , 'angularSpinner'
        , 'angularMoment'
        , 'matchMedia'
        , 'LocalStorageModule'
        , 'angularSpinner'
    ]);

    app.run([
        '$rootScope', '$location', '$window', '$route', '$timeout', 'screenSize', 'localStorageService', 'constants',
        function ($rootScope, $location, $window, $route, $timeout, screenSize, localStorageService, constants) {
            $rootScope.$on('$routeChangeStart', function(event, next, current) {
                $rootScope.loading = true;
            });

            $rootScope.$on('$routeChangeSuccess', function() {
                $rootScope.loading = false; 
                $rootScope.documentTitle = constants.title;
                if ($route.current.$$route && $route.current.$$route.title) {
                    $rootScope.title = $route.current.$$route.title;
                    $rootScope.documentTitle += " - " + $rootScope.title;
                }
            });

            $rootScope.xs = screenSize.on('xs', function(match) { $rootScope.xs = match; });
            $rootScope.sm = screenSize.on('sm', function(match) { $rootScope.sm = match; });
            $rootScope.md = screenSize.on('md', function(match) { $rootScope.md = match; });
            $rootScope.lg = screenSize.on('lg', function(match) { $rootScope.lg = match; });
            $rootScope.size = screenSize.get();
            screenSize.on('xs,sm,md,lg', function() {
                $rootScope.size = screenSize.get();
            });

            $rootScope.scrolled = false;
            $rootScope.scrollOffset = 0;
            angular.element($window).bind("scroll", function() {
                $rootScope.scrolled = this.pageYOffset > 0;
                $rootScope.scrollOffset = this.pageYOffset;
                $rootScope.$apply();
            });

            localStorageService.set('randomSuffix', Math.random());
            $rootScope.loading = false;

            $timeout(function() {
                angular.element(document.querySelector('#preloader')).addClass('die');
                $timeout(function () {
                    angular.element(document.querySelector('#preloader')).remove();//removeClass('die');
                }, 2000);
            }, 1000);
        }
    ]);
    return app;
})(APP);