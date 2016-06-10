(function () {
    'use strict';
    var ctrlId = 'contactCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'vcRecaptchaService', ctrlFactory]);
    function ctrlFactory($scope, constants, vcRecaptchaService) {
        $scope.reCaptchaPublicKey = constants.reCaptchaPublicKey;
    }
})();