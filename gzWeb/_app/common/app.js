var APP = (function () {
    'use strict';

    var id = 'GZ';

    var app = angular.module(id, [
        // Angular modules 
        'ngRoute'
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
        , 'angular-intro'
    ]);

    app.id = id;

    app.run([
        '$rootScope', '$location', '$route', '$timeout', 'screenSize', 'localStorageService', 'constants', 'auth', 'chat', 'helpers', 'nav', '$window',
        function ($rootScope, $location, $route, $timeout, screenSize, localStorageService, constants, auth, chat, helpers, nav, $window) {
            //var nbDigest = 0;
            //$rootScope.$watch(function () {
            //    nbDigest++;
            //    console.log("Digest cycles: " + nbDigest);
            //});

            function hidePreloader() {
                var preloader = document.getElementById("preloader");
                if (preloader) {
                    preloader.className = "die";
                    setTimeout(function () {
                        var body = document.getElementsByTagName("BODY")[0];
                        body.removeChild(preloader);
                        //setBodyRestHeight();
                    }, 1000);
                }
            }

            function showContent() {
                var content = document.getElementById("body-content");
                content.className = "ok";

                //var loading = document.getElementById("loading");
                //loading.className = "ok";

                var headerNav = document.getElementById("header-nav");
                headerNav.className += " navbar-fixed-top";

                var backToTop = document.getElementById("back-to-top");
                if (backToTop)
                    backToTop.style.display = "block";
            }

            function setBodyRestHeight() {
                var content = document.getElementById("body-content");
                var rest = document.getElementById("body-rest");
                var setHeight = function () {
                    var h = window.innerHeight - content.clientHeight;
                    if (h < 0)
                        h = 0;
                    rest.style.height = h + 'px';
                };
                angular.element($window).bind('resize', setHeight);
                $rootScope.$on('$routeChangeSuccess', setHeight);
                $timeout(setHeight);
            }

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

            function reveal() {
                hidePreloader();
                showContent();
                loadWebFonts();
            }

            function defaultBeforeSend(xhr, json) {
                var authData = localStorageService.get(constants.storageKeys.authData);
                if (authData)
                    xhr.setRequestHeader('Authorization', 'Bearer ' + authData.token);
            }

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
                    if (next && !next.redirectTo && !auth.authorize(next.roles))
                        $location.path(constants.routes.home.path);
                    else if (next.$$route.originalPath === constants.routes.home.path && auth.data.isGamer)
                        $location.path(constants.routes.games.path).search({});
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
                        angular.element('#header-nav').css('top', 0);
                    });
                }
                onRouteChangeSuccess();
                $rootScope.$on('$routeChangeSuccess', onRouteChangeSuccess);

                helpers.ui.watchScreenSize($rootScope);
                helpers.ui.watchWindowScroll($rootScope, function () {
                    if (!$rootScope.mobile && $rootScope.scrolled)
                        $('.zopim').addClass('scrolled');
                    else
                        $('.zopim').removeClass('scrolled');
                });
                chat.show();
                $rootScope.initialized = true;

                $rootScope.backToTop = function () {
                    $("html, body").animate({ scrollTop: 0 });
                };

                if (!auth.authorize($route.current.$$route.roles)) {
                    nav.setRequestUrl($location.$$url);
                    $location.path(constants.routes.home.path).search({});
                    $rootScope.redirected = true;
                }

                reveal();

                $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
                    $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                });
                $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
                    $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                });
                $rootScope.$broadcast(constants.events.ON_AFTER_INIT);
            }

            function onInit() {
                if ($route.current.$$route.originalPath === constants.routes.home.path && auth.data.isGamer)
                    $location.path(constants.routes.games.path).search({});
                helpers.ui.compile({ selector: '#footer', templateUrl: '_app/common/footer.html', controllerId: 'footerCtrl' });
                helpers.ui.compile({ selector: '#header', templateUrl: '_app/common/header.html', controllerId: 'headerCtrl', callback: onInitCallback });
            }

            function run() {
                $rootScope.$on(constants.events.ON_INIT, onInit);

                JL.setOptions({ "defaultBeforeSend": defaultBeforeSend });
                $rootScope.initialized = false;
                $rootScope.redirected = false;
                $rootScope.mobile = helpers.ui.isMobile();
                $rootScope.defaultImg = constants.defaultImg;
                localStorageService.set(constants.storageKeys.randomSuffix, Math.random());
                auth.init();
            }

            run();
        }
    ]);

    return app;
})(APP);