(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'emWamp', 'api', 'localStorageService', 'constants', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, emWamp, api, localStorageService, constants) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.responseMsg = null;

        $scope.submit = function () {
            if ($scope.form.$valid)
                login();
        };
        function login(){
            $scope.waiting = true;
            $scope.errorMsg = "";

            var emResponse = emWamp.login({
                usernameOrEmail: $scope.model.usernameOrEmail,
                password: $scope.model.password
            });

            emResponse.then(function (emResult) {
                var gzResponse = api.login($scope.model.usernameOrEmail, $scope.model.password);

                gzResponse.then(function (gzResult) {
                    localStorageService.set(constants.storageKeys.authData, {
                        username: gzResult.data.userName,
                        token: gzResult.data.access_token
                    });
                    $rootScope.$broadcast(constants.events.SESSION_STATE_CHANGE);
                    $scope.nsOk();
                    $scope.waiting = false;
                }, function (error) {
                    emWamp.logout();
                    $scope.errorMsg = error.data.error_description;
                    $scope.waiting = false;
                });
            }, function (error) {
                $scope.errorMsg = error.desc;
                $scope.waiting = false;
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