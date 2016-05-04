(function () {
    'use strict';
    var ctrlId = 'registerStartCtrl';
    APP.controller(ctrlId, ["$scope", "$http", "$filter", "emWamp", "$timeout", ctrlFactory]);
    function ctrlFactory($scope, $http, $filter, emWamp, $timeout) {
        $scope.spinnerOptions = { radius: 5, width: 2, length: 4, color: '#fff', position: 'absolute', top: '50%', right: 0 };
        $scope.model = {
            email: null,
            username: null,
            password: null
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
                    logError(error);
                });
            }
        };
        $scope.onEmailFocus = function () {
            $scope.emailFocused = true;
            //$scope.resetEmailValidation();
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
                //$scope.usernameValidation.isAvailable = true;
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
                    logError(error);
                });
            }
        };
        $scope.onUsernameFocus = function () {
            $scope.usernameFocused = true;
            //$scope.resetUsernameValidation();
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
                _passwordPolicyRegEx = new RegExp(result.regularExpression);
                _passwordPolicyError = result.message;
            }, logError);
        };

        $scope.passwordValidation = {
            //isValidating: false,
            isValid: true,
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
        $scope.onPasswordFocus = function () {
            $scope.passwordFocused = true;
            //$scope.resetPasswordValidation();
        }
        $scope.onPasswordBlur = function () {
            $scope.passwordFocused = false;
            $scope.validatePassword($scope.model.password);
        }
        // #endregion
        
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
                proccedToUserDetails();
        };
        function proccedToUserDetails(){
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerDetails.html',
                nsCtrl: 'registerDetailsCtrl',
                nsStatic: true,
                nsParams: { startModel: $scope.model }
            });
        }

        function logError(error) {
            console.log(error);
        };

        function init() {
            getPasswordPolicy();
        };
        init();
    }
})();