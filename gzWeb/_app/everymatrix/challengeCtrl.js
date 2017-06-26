(function () {
    'use strict';
    var ctrlId = 'challengeCtrl';
    APP.controller(ctrlId, ['$scope', 'localStorageService', 'constants', 'vcRecaptchaService', ctrlFactory]);
    function ctrlFactory($scope, localStorageService, constants, vcRecaptchaService) {
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
        var _widgetId = undefined;
        $scope.setWidgetId = function (widgetId) {
            _widgetId = widgetId;
        };
        $scope.model = {
            recaptcha: undefined
        }
        $scope.submit = function () {
            $scope.nsOk(vcRecaptchaService.getResponse(widgetId));
        };
    }
})();