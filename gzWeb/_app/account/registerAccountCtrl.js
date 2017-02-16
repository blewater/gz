(function () {
    'use strict';
    var ctrlId = 'registerAccountCtrl';
    APP.controller(ctrlId, ["$scope", "$filter", "emWamp", "$timeout", "$log", ctrlFactory]);
    function ctrlFactory($scope, $filter, emWamp, $timeout, $log) {
        $scope.spinnerOptions = { radius: 5, width: 2, length: 4, color: '#fff', position: 'absolute', top: '50%', right: 0 };
        $scope.model = {
            email: null,
            username: null,
            password: null,
            confirmPassword: null
        };

        // #region Email
        $scope.emailValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.resetEmailValidation = function () {
            $scope.emailValidation.isValidating = false;
            $scope.emailValidation.isAvailable = true;
            $scope.emailValidation.error = '';
        };
        $scope.validateEmail = function (email) {
            if (!$scope.emailValidation.isValidating) {
                $scope.emailValidation.isValidating = true;
                emWamp.validateEmail(email).then(function (result) {
                    $scope.emailValidation.isValidating = false;
                    $scope.emailValidation.isAvailable = result.isAvailable;
                    $scope.emailValidation.error = result.error;
                }, function (error) {
                    $scope.emailValidation.isValidating = false;
                    $log.error(error.desc);
                });
            }
        };
        $scope.onEmailFocus = function () {
            $scope.emailFocused = true;
        }
        $scope.onEmailBlur = function () {
            $scope.emailFocused = false;
            $scope.validateEmail($scope.model.email);
        }
        // #endregion

        // #region Username
        $scope.usernameValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.resetUsernameValidation = function () {
            $scope.usernameValidation.isValidating = false;
            $scope.usernameValidation.isAvailable = true;
            $scope.usernameValidation.error = '';
        };
        $scope.validateUsername = function (username) {
            if (!$scope.usernameValidation.isValidating) {
                $scope.usernameValidation.error = '';
                $scope.usernameValidation.isValidating = true;
                emWamp.validateUsername(username).then(function (result) {
                    $timeout(function() {
                        $scope.usernameValidation.isValidating = false;
                        $scope.usernameValidation.isAvailable = result.isAvailable;
                        $scope.usernameValidation.error = result.error;
                    }, 0);
                }, function (error) {
                    $scope.usernameValidation.isValidating = false;
                    $log.error(error.desc);
                });
            }
        };
        $scope.onUsernameFocus = function () {
            $scope.usernameFocused = true;
        }
        $scope.onUsernameBlur = function () {
            $scope.usernameFocused = false;
            $scope.validateUsername($scope.model.username);
        }
        // #endregion

        // #region Password
        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function getPasswordPolicy() {
            emWamp.getPasswordPolicy().then(function (result) {
                //_passwordPolicyRegEx = new RegExp("(?=.*[0-9]+)(?=.*[A-Za-z]+)(?=.*[*:%!~]+).{8,20}");
                _passwordPolicyRegEx = new RegExp(result.regularExpression);
                _passwordPolicyError = result.message;
            }, function(error) {
                $log.error(error.desc);
            });
        };

        $scope.passwordValidation = {
            isValid: undefined,
            error: ''
        };
        $scope.resetPasswordValidation = function() {
            $scope.passwordValidation.isValid = true;
            $scope.passwordValidation.error = '';
        };
        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                $scope.passwordValidation.isValid = true;
            } else {
                $scope.passwordValidation.isValid = false;
                $scope.passwordValidation.error = _passwordPolicyError;
            };
        };
        $scope.passwordFocusedOnce = false;
        $scope.onPasswordFocus = function () {
            $scope.passwordFocused = true;
            $scope.passwordFocusedOnce = true;
        }
        $scope.onPasswordBlur = function () {
            $scope.passwordFocused = false;
            $scope.validatePassword($scope.model.password);
        }

        //$scope.passwordValidOnce = false;
        //var unregisterIsPasswordValidWatch = $scope.$watch(function () {
        //    return $scope.form.password.$dirty && $scope.form.password.$valid && $scope.passwordValidation.isValid === true;
        //}, function (newValue, oldValue) {
        //    if (newValue === true) {
        //        $scope.passwordValidOnce = true;
        //        unregisterIsPasswordValidWatch();
        //    }
        //});

        // #endregion
        
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
                proccedToUserDetails();
        };
        function proccedToUserDetails(){
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/registerDetails.html',
                nsCtrl: 'registerDetailsCtrl',
                nsStatic: true,
                nsParams: {
                    startModel: {
                        email: $scope.model.email,
                        username: $scope.model.username,
                        password: $scope.model.password
                    }
                }
            });
        }

        function init() {
            getPasswordPolicy();
            window.appInsights.trackPageView("REGISTER ACCOUNT");
        };
        init();
    }
})();