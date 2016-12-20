(function () {
    'use strict';
    var ctrlId = 'footerCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', '$location', 'accountManagement', '$controller', '$rootScope', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants, $location, accountManagement, $controller, $rootScope) {
        $controller('authCtrl', { $scope: $scope });

        $scope.menu = [
            { route: constants.routes.transparency, when: function () { return true; } },
            { route: constants.routes.about, when: function () { return true; } },
            { route: constants.routes.faq, when: function () { return $rootScope.routeData && ($rootScope.routeData.investing || $rootScope.routeData.wandering); } },
            { route: constants.routes.help, when: function () { return $rootScope.routeData && ($rootScope.routeData.gaming || $rootScope.routeData.wandering); } },
            { route: constants.routes.privacy, when: function () { return $rootScope.routeData && ($rootScope.routeData.gaming || $rootScope.routeData.wandering); } },
            { route: constants.routes.terms, when: function () { return true; } },
            { route: constants.routes.promotions, when: function () { return true; } }
        ];
        $scope.getClass = function (path) {
            return $location.path() === path ? 'focus' : '';
        }

        $scope.copyrightYears = (function () {
            var startingYear = 2016;
            var currentYear = moment().year();
            return currentYear === startingYear ? startingYear : startingYear + " - " + currentYear;
        })();
        $scope.inDebugMode = localStorageService.get(constants.storageKeys.debug);

        $scope.gotoResponsibleGaming = function () {
            accountManagement.open(accountManagement.states.responsibleGaming);
        };

        $scope._init(loadAuthData);

        function loadAuthData() {
            $scope.isGamer = $scope._authData.isGamer;
        }
    }
})();