(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'emWamp', 'api', 'localStorageService', 'constants', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, emWamp, api, localStorageService, constants) {
        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.responseMsg = null;

        $scope.login = function () {
            if ($scope.form.$valid) {
                $scope.errorMsg = "";
                //emWamp.logout().then(function() {
                //    api.logout();

                //});
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
                    }, function (error) {
                        $scope.errorMsg = error.desc;
                    });

                }, function (error) {
                    $scope.errorMsg = error.desc;
                });

            }
        };

        //function emLogin(username, password) {
        //    return emWamp.login(username, password);
        //};

        //function gzLogin(username, password) {
        //    return (username, password);
        //};
    }
})();