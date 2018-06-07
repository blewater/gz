(function () {
    'use strict';

    APP.factory('footerMenu', ['constants', '$rootScope', serviceFactory]);
    function serviceFactory(constants, $rootScope) {
        function getMenu () {
            return [
                { route: constants.routes.transparency, when: function () { return true; } },
                { route: constants.routes.about, when: function () { return true; } },
                { route: constants.routes.faq, when: function () { return true; } },
                { route: constants.routes.help, when: function () { return true; } },
                {
                    title: "Privacy Policy",
                    subroutes: [
                        { route: constants.routes.privacyGames, when: function () { return $rootScope.routeData && ($rootScope.routeData.gaming || $rootScope.routeData.wandering); } },
                        { route: constants.routes.privacyInvestment, when: function () { return $rootScope.routeData && $rootScope.routeData.investing; } },
                    ],
                    when: function () { return true; }
                },
                {
                    title: "Terms & Conditions",
                    subroutes: [
                        { route: constants.routes.termsGames, when: function () { return $rootScope.routeData && ($rootScope.routeData.gaming || $rootScope.routeData.wandering); } },
                        { route: constants.routes.termsInvestment, when: function () { return $rootScope.routeData && $rootScope.routeData.investing; } }
                    ],
                    when: function () { return true; }
                },
                { route: constants.routes.cookiePolicy, when: function () { return true; } },
                { route: constants.routes.thirdParties, when: function () { return true; } },
                { route: constants.routes.promotions, when: function () { return true; } },
                { route: { path: "https://affiliates.greenzorro.com/", title: "Affiliates" }, when: function () { return true; }, inNewTab: true }
            ];
        }

        var service = {
            getMenu: getMenu,
        };

        return service;
    };
})();