(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'emWamp', 'auth', 'localStorageService', 'constants', 'message', 'modals', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, emWamp, auth, localStorageService, constants, message, modals) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
        var _widgetId = undefined;
        $scope.setWidgetId = function (widgetId) {
            _widgetId = widgetId;
        };
        $scope.waiting = false;
        $scope.responseMsg = null;

        $scope.usernameOrEmailContainsAt = function () {
            return $scope.model.usernameOrEmail && $scope.model.usernameOrEmail.indexOf('@') !== -1;
        };

        $scope.model = {
            usernameOrEmail: undefined,
            password: undefined,
            showCaptcha: false,
            recaptcha: undefined
        };

        // Turning application offline: Sept 1
        //$scope.submit = function () {
        //    if ($scope.form.$valid && !$scope.waiting)
        //        login();
        //};

        function login() {
            $scope.waiting = true;
            $scope.errorMsg = "";

            auth.login($scope.model.usernameOrEmail, $scope.model.password, $scope.model.showCaptcha, _widgetId).then(function (response) {
                if (response.enterCaptcha) {
                    $scope.waiting = false;
                    $scope.model.showCaptcha = true;
                }
                else {
                    $scope.waiting = false;
                    if (response.emLogin === false)// && response.gzLogin === false)
                        $scope.errorMsg = response.emError;// "The login failed. Please check your username/email and password.";
                    else {
                        //if (response.emLogin === false)
                        //    message.error("We have experienced technical difficulty in accessing our online games. Please try again shortly by pressing the ​<i>\'Retry to connect\'</i>​​ button.");
                        //if (response.gzLogin === false)
                        //    message.error("We have experienced technical difficulty in accessing your investment pages. Please try again later by pressing the ​<i>\'Retry to connect\'</i>​ button.");

                        $scope.nsOk();
                    }
                }
            });
        }

        $scope.forgotPassword = function () {
            modals.forgotPassword($scope.nsNext);
            //$scope.nsNext({
            //    nsType: 'modal',
            //    nsSize: '600px',
            //    nsTemplate: '_app/account/forgotPassword.html',
            //    nsCtrl: 'forgotPasswordCtrl',
            //    nsStatic: true
            //});
        };

        $scope.signup = function () {
            //helpers.utils.ga(constants.gaKeys.signup);
            modals.register($scope.nsNext);
            //$scope.nsNext({
            //    nsType: 'modal',
            //    nsSize: '600px',
            //    nsTemplate: '_app/account/registerAccount.html',
            //    nsCtrl: 'registerAccountCtrl',
            //    nsStatic: true
            //});
        };

        window.appInsights.trackPageView("LOGIN");
    }
})();