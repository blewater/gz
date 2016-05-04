(function () {
    "use strict";
    var ctrlId = "resetPasswordCtrl";
    APP.controller(ctrlId, ['$scope', '$http', 'constants', 'emWamp', 'message', '$location', ctrlFactory]);
    function ctrlFactory($scope, $http, constants, emWamp, message, $location) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.model = {
            key: $scope.resetKey,
            code: $scope.resetCode,
            isKeyAvailable: undefined,
            password: undefined,
            confirmPassword: undefined
        }
        
        // #region Password
        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function getPasswordPolicy() {
            emWamp.getPasswordPolicy().then(function (result) {
                //_passwordPolicyRegEx = new RegExp("(?=.*[0-9]+)(?=.*[A-Za-z]+)(?=.*[*:%!~]+).{8,20}");
                _passwordPolicyRegEx = new RegExp(result.regularExpression);
                _passwordPolicyError = result.message;
            }, function(error) {
                console.log(error);
            });
        };

        $scope.passwordValidation = {
            isValid: undefined,
            error: ''
        };
        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                $scope.passwordValidation.isValid = true;
            } else {
                $scope.passwordValidation.isValid = false;
                $scope.passwordValidation.error = _passwordPolicyError;
            };
        };
        $scope.onPasswordFocus = function () {
            $scope.passwordFocused = true;
        }
        $scope.onPasswordBlur = function () {
            $scope.passwordFocused = false;
            $scope.validatePassword($scope.model.password);
        }

        $scope.passwordValidOnce = false;
        var unregisterIsPasswordValidWatch = $scope.$watch(function () {
            return $scope.form.password.$dirty && $scope.form.password.$valid && $scope.passwordValidation.isValid === true;
        }, function (newValue, oldValue) {
            if (newValue === true) {
                $scope.passwordValidOnce = true;
                unregisterIsPasswordValidWatch();
            }
        });
        // #endregion

        $scope.forgotPassword = function () {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/forgotPassword.html',
                nsCtrl: 'forgotPasswordCtrl',
                nsStatic: true
            });
        };
        
        $scope.submit = function () {
            if ($scope.form.$valid)
                reset();
        };
        function reset(){
            $scope.waiting = true;
            emWamp.resetPassword($scope.model.key, $scope.model.password)
                .then(function(emResult) {
                        $http({
                            url: 'api/Account/ResetPassword',
                                method: 'POST',
                                data: {
                                    Email: $scope.model.email,
                                    Password: $scope.model.password,
                                    ConfirmPassword: $scope.model.password,
                                    Code: $scope.model.code
                                }
                            })
                            .then(function(gzResult) {
                                    $scope.waiting = false;
                                    message.toastr("Your password has been reset successfully!");
                                    $location.search('');
                                    $scope.nsOk(true);
                                },
                                function(error) {
                                    $scope.model.isKeyAvailable = false;
                                    $scope.waiting = false;
                                    console.log(error);
                                });
                    },
                    function(error) {
                        $scope.model.isKeyAvailable = false;
                        $scope.waiting = false;
                        console.log(error);
                    });
        }

        function checkResetKeyAvailability() {
            emWamp.isResetPwdKeyAvailable($scope.model.key).then(function (result) {
                $scope.model.isKeyAvailable = result;
                getPasswordPolicy();
            }, function (error) {
                $scope.model.isKeyAvailable = false;
                console.log(error);
            });
        }

        function init() {
            checkResetKeyAvailability();
        };
        init();
    }
})();