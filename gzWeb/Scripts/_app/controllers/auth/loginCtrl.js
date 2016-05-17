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
            $scope.errorMsg = "";

            auth.login($scope.model.usernameOrEmail, $scope.model.password).then(function (response) {
                $scope.waiting = false;
                if (response.emLogin === true && response.gzLogin === true)
                    $scope.nsOk();
                else {
                    if (response.emLogin === false)
                        $scope.errorMsg += response.emError;
                    if (response.emLogin === false && response.gzLogin === false)
                        $scope.errorMsg += "<br/>";
                    if (response.gzLogin === false)
                        $scope.errorMsg += response.gzError;
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