(function () {
    'use strict';
    var ctrlId = 'challengeCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants) {
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
        $scope.submit = function () {
            $scope.nsOk();
        };
    }
})();