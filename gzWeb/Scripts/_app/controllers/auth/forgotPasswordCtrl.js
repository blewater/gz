(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope.model = {
            email: null
        };

        $scope.backToLogin = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: 'sm',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        $scope.sendInstructions = function () {
            if ($scope.form.$valid) {
            }
        };
    }
})();