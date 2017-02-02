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
        , 'angular-appinsights'
    ]);

    app.id = id;

    app.run([
        '$rootScope', '$location', '$route', '$timeout', 'screenSize', 'localStorageService', 'constants', 'auth', 'chat', 'helpers', 'nav',
        function ($rootScope, $location, $route, $timeout, screenSize, localStorageService, constants, auth, chat, helpers, nav) {

            function hidePreloader() {
                var preloader = document.getElementById("preloader");
                preloader.className = "die";
                setTimeout(function () {
                    var body = document.getElementsByTagName("BODY")[0];
                    body.removeChild(preloader);
                }, 1000);
            };

            function showContent() {
                var content = document.getElementById("body-content");
                content.className = "ok";

                //var loading = document.getElementById("loading");
                //loading.className = "ok";

                var headerNav = document.getElementById("header-nav");
                headerNav.className += " navbar-fixed-top";
            };

            function loadWebFonts() {
                WebFont.load({
                    google: {
                        families: ['Fira Sans']
                    },
                    active: function () {
                        $rootScope.fontsLoaded = true;
                    }
                });
            }

            function reveal () {
                hidePreloader();
                showContent();
                loadWebFonts();
            }

            function defaultBeforeSend(xhr, json) {
                var authData = localStorageService.get(constants.storageKeys.authData);
                if (authData)
                    xhr.setRequestHeader('Authorization', 'Bearer ' + authData.token);
            };

            function onInitCallback() {
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
                        var title = constants.title;
                        if ($route.current.$$route) {
                            if ($route.current.$$route.title)
                                title += " - " + $route.current.$$route.title;
                            setRouteData($route.current.$$route);
                        }
                        document.title = title;
                    });
                };
                onRouteChangeSuccess();
                $rootScope.$on('$routeChangeSuccess', onRouteChangeSuccess);

                helpers.ui.watchScreenSize($rootScope);
                helpers.ui.watchWindowScroll($rootScope);
                chat.show();
                $rootScope.initialized = true;

                if (!auth.authorize($route.current.$$route.roles)) {
                    nav.setRequestUrl($location.$$path);
                    $location.path(constants.routes.home.path);
                    $rootScope.redirected = true;
                }

                reveal();

                $rootScope.$broadcast(constants.events.ON_AFTER_INIT);
            }

            function onInit() {
                helpers.ui.compile({ selector: '#footer', templateUrl: '_app/common/footer.html', controllerId: 'footerCtrl' });
                helpers.ui.compile({ selector: '#header', templateUrl: '_app/common/header.html', controllerId: 'headerCtrl', callback: onInitCallback });
            }

            function run() {
                $rootScope.$on(constants.events.ON_INIT, onInit);

                JL.setOptions({ "defaultBeforeSend": defaultBeforeSend });
                $rootScope.initialized = false;
                $rootScope.redirected = false;
                $rootScope.mobile = helpers.ui.isMobile();
                localStorageService.set(constants.storageKeys.randomSuffix, Math.random());
                auth.init();
            }

            run();
        }
    ]);

    return app;
})(APP);