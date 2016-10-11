(function () {
    'use strict';
    var ctrlId = 'footerCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', '$location', 'accountManagement', '$controller', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants, $location, accountManagement, $controller) {
        $controller('authCtrl', { $scope: $scope });

        $scope.routes = [
            constants.routes.transparency,
            constants.routes.about,
            constants.routes.faq,
            constants.routes.help,
            constants.routes.privacy,
            constants.routes.terms,
            constants.routes.promotions
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