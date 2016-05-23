﻿(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'emWamp', 'auth', 'localStorageService', 'constants', 'message', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, emWamp, auth, localStorageService, constants, message) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.waiting = false;
        $scope.responseMsg = null;

        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.submit = function () {
            if ($scope.form.$valid)
                login();
        };

        function login(){
            $scope.waiting = true;
            $scope.errorMsg = "";

            auth.login($scope.model.usernameOrEmail, $scope.model.password).then(function (response) {
                $scope.waiting = false;
                if (response.emLogin === false && response.gzLogin === false)
                    $scope.errorMsg = "The login failed. Please check your username/email and password.";
                else
                    $scope.nsOk();

                if (response.emLogin === false)
                    message.error("We have experienced technical difficulty in accessing our online games. Please try again shortly by pressing the ​<i>\'Retry to connect\'</i>​​ button.");
                if (response.gzLogin === false)
                    message.error("We have experienced technical difficulty in accessing your investment pages. Please try again later by pressing the ​<i>\'Retry to connect\'</i>​ button.");

                //$scope.emErrorMsg = response.emError;
                //$scope.gzErrorMsg = response.gzError;
            });
        }

        $scope.forgotPassword = function () {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/forgotPassword.html',
                nsCtrl: 'forgotPasswordCtrl',
                nsStatic: true
            });
        };

        $scope.signup = function () {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerAccount.html',
                nsCtrl: 'registerAccountCtrl',
                nsStatic: true
            });
        };
    }
})();