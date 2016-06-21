(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'vcRecaptchaService', 'emWamp', 'auth', 'message', '$location', 'localStorageService', ctrlFactory]);
    function ctrlFactory($scope, constants, vcRecaptchaService, emWamp, auth, message, $location, localStorageService) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);

        $scope.emailValidOnce = false;
        var unregisterIsEmailValidWatch = $scope.$watch(function(){
            return $scope.form.email.$dirty && $scope.form.email.$valid; 
        }, function (newValue, oldValue) {
            if (newValue === true) {
                $scope.emailValidOnce = true;
                unregisterIsEmailValidWatch();
            }
        });

        $scope.model = {
            email: null
        };

        $scope.backToLogin = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        
        $scope.submit = function () {
            if ($scope.form.$valid)
                sendInstructions();
        };
        function sendInstructions(){
            $scope.waiting = true;
            auth.forgotPassword($scope.model.email).then(function (result) {
                $scope.waiting = false;
                message.success("You will receive an email at '" + $scope.model.email + "' that will guide you through the reset password process.");
                $scope.nsOk(true);
            }, function(error) {
                $scope.waiting = false;
                $scope.sendResetPasswordEmailError = error;
            });
        }
    }
})();