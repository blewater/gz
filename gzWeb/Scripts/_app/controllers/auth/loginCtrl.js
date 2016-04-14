(function () {
    'use strict';
    var ctrlId = 'loginCtrl';
    APP.controller(ctrlId, ['$scope', '$http', 'emWamp', ctrlFactory]);
    function ctrlFactory($scope, $http, emWamp) {
        $scope.model = {
            usernameOrEmail: null,
            password: null
        };

        $scope.responseMsg = null;

        $scope.login = function() {
            var gzResponse = gzLogin($scope.model.usernameOrEmail, $scope.model.password);
            gzResponse.then(function (result) {
                sessionStorage.setItem("userName", result.data.userName);
                sessionStorage.setItem("accessToken", result.data.access_token);
                $scope.responseMsg = "Ok";
                console.log(result);
            }, function(result) {
                console.log(result);
            });
        };

        function emLogin() {
            
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