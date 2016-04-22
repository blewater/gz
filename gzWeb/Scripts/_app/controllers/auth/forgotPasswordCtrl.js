(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'chat', ctrlFactory]);
    function ctrlFactory($scope, chat) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.model = {
            email: null
        };

        $scope.backToLogin = function () {
            //chat.start();
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
                //$scope.loading = true;
            }
        };
    }
})();