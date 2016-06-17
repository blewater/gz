(function () {
    'use strict';
    var ctrlId = 'contactCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'localStorageService', 'vcRecaptchaService', ctrlFactory]);
    function ctrlFactory($scope, constants, localStorageService, vcRecaptchaService) {
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
    }
})();