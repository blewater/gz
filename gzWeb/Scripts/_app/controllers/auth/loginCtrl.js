(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ["$scope", "$http", "emWamp", "api", "localStorageService", ctrlFactory]);
    function ctrlFactory($scope, $http, emWamp, api, localStorageService) {
        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.responseMsg = null;

        $scope.login = function () {
            if ($scope.form.$valid) {
                $scope.invalidCredentials = false;
                $scope.responseMsg = "";

                var emResponse = emWamp.login({
                    usernameOrEmail: $scope.model.usernameOrEmail,
                    password: $scope.model.password
                });

                emResponse.then(function (emResult) {
                    $scope.responseMsg = angular.toJson(emResult, true);

                    var gzResponse = api.login($scope.model.usernameOrEmail, $scope.model.password);
                    gzResponse.then(function (gzResult) {
                        localStorageService.set("userName", gzResult.data.userName);
                        localStorageService.set("accessToken", gzResult.data.access_token);
                        $scope.responseMsg += "Ok";
                        console.log(gzResult);
                    }, function (error) {
                        $scope.invalidCredentials = true;
                        $scope.responseMsg = error;
                        console.log(error);
                    });

                }, function (error) {
                    $scope.invalidCredentials = true;
                    $scope.responseMsg = error.desc;
                    console.log(error);
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