(function () {
    'use strict';
    var ctrlId = 'footerCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants) {
        $scope.copyrightYears = (function () {
            var startingYear = 2016;
            var currentYear = moment().year();
            return currentYear === startingYear ? startingYear : startingYear + " - " + currentYear;
        })();
        $scope.inDebugMode = localStorageService.get(constants.storageKeys.debug);
    }
})();