(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', '$http', 'constants', 'vcRecaptchaService', 'emWamp', 'message', '$location', ctrlFactory]);
    function ctrlFactory($scope, $http, constants, vcRecaptchaService, emWamp, message, $location) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = constants.reCaptchaPublicKey;

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

            var promise = $http({
                url: 'api/Account/ForgotPassword',
                method: 'POST',
                data: {
                    Email: $scope.model.email
                }
            });

            promise.then(function(resetCode) {
            var changePwdUrl = $location.protocol() + "://" + $location.host();
            if ($location.port() > 0)
                changePwdUrl += ":" + $location.port();

                    changePwdUrl += "?";
                    changePwdUrl += "resetCode=";
                    changePwdUrl += resetCode;
                    changePwdUrl += "&";
                    changePwdUrl += "resetKey=";

            emWamp.sendResetPwdEmail({
                email: $scope.model.email,
                changePwdURL: changePwdUrl,
                captchaPublicKey: $scope.reCaptchaPublicKey,
                captchaChallenge: "",
                captchaResponse: vcRecaptchaService.getResponse()
                        })
                        .then(function(result) {
                $scope.waiting = false;
                                message.notify("You will receive an email at '" +
                                    $scope.model.email +
                                    "' that will guide you through the reset password process.");
                //vcRecaptchaService.reload();
                $scope.nsOk(true);
                            },
                            function(error) {
                                $scope.waiting = false;
                                $scope.sendResetPasswordEmailError = error;
                                vcRecaptchaService.reload();
                            });
                },
                function(error) {
                $scope.waiting = false;
                $scope.sendResetPasswordEmailError = error.desc;
                vcRecaptchaService.reload();
            });
        }
    }
})();