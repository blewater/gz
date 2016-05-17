(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'emWamp', 'auth', 'localStorageService', 'constants', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, emWamp, auth, localStorageService, constants) {
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
            $scope.emErrorMsg = "";
            $scope.gzErrorMsg = "";

            auth.login($scope.model.usernameOrEmail, $scope.model.password).then(function (response) {
                $scope.waiting = false;
                if (response.emLogin === true && response.gzLogin === true)
                    $scope.nsOk();
                else {
                    $scope.emErrorMsg = response.emError;
                    $scope.gzErrorMsg = response.gzError;
                }
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