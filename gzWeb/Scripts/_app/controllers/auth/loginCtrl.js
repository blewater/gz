(function () {
    "use strict";
    var ctrlId = "loginCtrl";
    APP.controller(ctrlId, ["$scope", "$http", "emWamp", "localStorageService", ctrlFactory]);
    function ctrlFactory($scope, $http, emWamp, localStorageService) {
        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.responseMsg = null;

        $scope.login = function () {
            $scope.responseMsg = "";
            var emResponse = emLogin($scope.model.usernameOrEmail, $scope.model.password);
            emResponse.then(function(emResult) {
                $scope.responseMsg = angular.toJson(emResult, true);

                var gzResponse = gzLogin($scope.model.usernameOrEmail, $scope.model.password);
                gzResponse.then(function (gzResult) {
                    localStorageService.set("userName", gzResult.data.userName);
                    localStorageService.set("accessToken", gzResult.data.access_token);
                    $scope.responseMsg += "Ok";
                    console.log(gzResult);
                }, function (error) {
                    console.log(error);
                });

            }, function(error) {
                // TODO: ...
                console.log(error);
            });
        };

        function emLogin(username, password) {
            return emWamp.userLogin(username, password);
        };

        function gzLogin(username, password) {
            return $http({
                url: "/TOKEN",
                method: "POST",
                data: $.param({
                    grant_type: "password",
                    username: username,
                    password: password
                }),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            });
        };
    }
})();