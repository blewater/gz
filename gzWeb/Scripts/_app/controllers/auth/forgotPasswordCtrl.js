(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ["$scope", "$http", "emWamp", "api", "localStorageService", ctrlFactory]);
    function ctrlFactory($scope, $http, emWamp, api, localStorageService) {
        $scope.model = {
            email: null
        };

        $scope.backToLogin = function () {
        };
        $scope.sendInstructions = function () {
            if ($scope.form.$valid) {
            }
        };
    }
})();