(function () {
    'use strict';
    var ctrlId = 'footerCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', '$location', 'accountManagement', '$controller', 'footerMenu', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants, $location, accountManagement, $controller, footerMenu) {
        $controller('authCtrl', { $scope: $scope });

        $scope.menu = footerMenu.getMenu();

        $scope.getClass = function (path) {
            return $location.path() === path ? 'focus' : '';
        }

        $scope.copyrightYears = (function () {
            var startingYear = 2016;
            var currentYear = moment().year();
            return currentYear === startingYear ? startingYear : startingYear + " - " + currentYear;
        })();

        $scope.gotoResponsibleGaming = function () {
            accountManagement.open(accountManagement.states.responsibleGaming);
        };

        $scope._init(loadAuthData);

        function loadAuthData() {
            $scope.isGamer = $scope._authData.isGamer;
            $scope.inDebugMode = localStorageService.get(constants.storageKeys.debug);
            $scope.version = localStorageService.get(constants.storageKeys.version);
        }
    }
})();