(function () {
    "use strict";
    var ctrlId = "resetPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'auth', 'message', '$location', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, auth, message, $location) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.model = {
            email: $scope.email,
            emKey: $scope.emKey,
            gzKey: $scope.gzKey,
            isKeyAvailable: undefined,
            password: undefined,
            confirmPassword: undefined
        }
        
        // #region Password
        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function getPasswordPolicy() {
            emWamp.getPasswordPolicy().then(function (result) {
                _passwordPolicyRegEx = new RegExp(result.regularExpression);
                _passwordPolicyError = result.message;
            }, function(error) {
                console.log(error.desc);
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

        $scope.forgotPassword = function () {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/forgotPassword.html',
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
            auth.resetPassword($scope.model).then(function (response) {
                $scope.waiting = false;
                message.success("Your password has been reset successfully!", { nsType: 'toastr' });
                $location.search('');
                $scope.nsOk(true);
            }, function(error) {
                $scope.model.isKeyAvailable = false;
                $scope.waiting = false;
                console.log(error);
            });
        }

        function checkResetKeyAvailability() {
            emWamp.isResetPwdKeyAvailable($scope.model.emKey).then(function (result) {
                $scope.model.isKeyAvailable = result;
                getPasswordPolicy();
            }, function (error) {
                $scope.model.isKeyAvailable = false;
                console.log(error.desc);
            });
        }

        function init() {
            checkResetKeyAvailability();
        };
        init();
    }
})();