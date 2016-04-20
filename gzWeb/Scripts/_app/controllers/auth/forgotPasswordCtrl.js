(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
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