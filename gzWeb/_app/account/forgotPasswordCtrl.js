(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'vcRecaptchaService', 'emWamp', 'auth', 'message', '$location', 'localStorageService', 'modals', ctrlFactory]);
    function ctrlFactory($scope, constants, vcRecaptchaService, emWamp, auth, message, $location, localStorageService, modals) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
        var _widgetId = undefined;
        $scope.setWidgetId = function (widgetId) {
            _widgetId = widgetId;
        };

        //$scope.emailValidOnce = false;
        //var unregisterIsEmailValidWatch = $scope.$watch(function(){
        //    return $scope.form.email.$dirty && $scope.form.email.$valid; 
        //}, function (newValue, oldValue) {
        //    if (newValue === true) {
        //        $scope.emailValidOnce = true;
        //        unregisterIsEmailValidWatch();
        //    }
        //});

        $scope.model = {
            email: null,
            recaptcha: undefined
        };

        $scope.backToLogin = function () {
            modals.login($scope.nsBack);
        };
        
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                sendInstructions();
        };

        function sendInstructions() {
            $scope.waiting = true;
            auth.forgotPassword($scope.model.email, _widgetId).then(sendCallback, sendCallback);
        }
        function sendCallback() {
            $scope.waiting = false;
            message.success("If a matching account was found, an email was sent to '" + $scope.model.email + "' to guide you through the reset password process.");
            $scope.nsOk(true);
        }

        function init() {
            if ($scope.email) {
                $scope.model.email = $scope.email;
                //$scope.emailValidOnce = true;
            }
            window.appInsights.trackPageView("FORGOT PASSWORD");
        }
        init();
    }
})();