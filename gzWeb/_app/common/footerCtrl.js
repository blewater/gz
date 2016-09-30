(function () {
    'use strict';
    var ctrlId = 'footerCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', '$location', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants, $location) {
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
    }
})();