(function () {
    "use strict";
    var ctrlId = "changePasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'auth', 'message', 'vcRecaptchaService', 'localStorageService', '$log', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, auth, message, vcRecaptchaService, localStorageService, $log) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
        var _widgetId = undefined;
        $scope.setWidgetId = function (widgetId) {
            _widgetId = widgetId;
        };

        $scope.model = {
            oldPassword: undefined,
            newPassword: undefined,
            confirmPassword: undefined,
            recaptcha: undefined
        }
        
        // #region Password
        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function getPasswordPolicy() {
            emWamp.getPasswordPolicy().then(function (result) {
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
        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                $scope.passwordValidation.isValid = true;
            } else {
                $scope.passwordValidation.isValid = false;
                $scope.passwordValidation.error = _passwordPolicyError;
            };
        };
        $scope.newPasswordFocusedOnce = false;
        $scope.onNewPasswordFocus = function () {
            $scope.newPasswordFocused = true;
            $scope.newPasswordFocusedOnce = true;
        }
        $scope.onNewPasswordBlur = function () {
            $scope.newPasswordFocused = false;
            $scope.validatePassword($scope.model.newPassword);
        }

        //$scope.passwordValidOnce = false;
        //var unregisterIsPasswordValidWatch = $scope.$watch(function () {
        //    return $scope.form.newPassword.$dirty && $scope.form.newPassword.$valid && $scope.passwordValidation.isValid === true;
        //}, function (newValue, oldValue) {
        //    if (newValue === true) {
        //        $scope.passwordValidOnce = true;
        //        unregisterIsPasswordValidWatch();
        //    }
        //});
        // #endregion
        
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                change();
        };
        function change(){
            $scope.waiting = true;
            auth.changePassword($scope.model.oldPassword, $scope.model.newPassword, $scope.model.confirmPassword, _widgetId).then(function (result) {
                window.appInsights.trackEvent("CHANGE PASSWORD");
                $scope.waiting = false;
                message.success("Your password has been changed successfully!", { nsType: 'toastr' });
                $scope.nsOk(true);
            }, function (error) {
                $scope.waiting = false;
                $scope.changePasswordError = error;
            });
        }

        function init() {
            getPasswordPolicy();
        };
        init();
    }
})();