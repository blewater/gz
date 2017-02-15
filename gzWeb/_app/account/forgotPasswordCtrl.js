(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'vcRecaptchaService', 'emWamp', 'auth', 'message', '$location', 'localStorageService', ctrlFactory]);
    function ctrlFactory($scope, constants, vcRecaptchaService, emWamp, auth, message, $location, localStorageService) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);

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
            email: null
        };

        $scope.backToLogin = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        
        $scope.submit = function () {
            if ($scope.form.$valid)
                sendInstructions();
        };

        function sendInstructions() {
            $scope.waiting = true;
            auth.forgotPassword($scope.model.email).then(sendCallback, sendCallback);
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