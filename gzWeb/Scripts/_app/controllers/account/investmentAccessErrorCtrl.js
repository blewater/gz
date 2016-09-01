(function () {
    "use strict";
    var ctrlId = "investmentAccessErrorCtrl";
    APP.controller(ctrlId, ['$scope', 'auth', 'constants', 'message', '$location', ctrlFactory]);
    function ctrlFactory($scope, auth, constants, message, $location) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.waiting = false;

        $scope.model = {
            usernameOrEmail: auth.data.username,
            password: null
        };

        $scope.submit = function () {
            if ($scope.form.$valid)
                login();
        };

        function login() {
            $scope.waiting = true;
            $scope.errorMsg = "";

            auth.gzLogin($scope.model.usernameOrEmail, $scope.model.password).then(function (response) {
                $scope.waiting = false;
                $location.path(constants.routes.summary.path);
            }, function () {
                $scope.waiting = false;
                $scope.nsCancel();
                message.error("We are still experiencing technical difficulties. Please try again later to login.")
            });
        }
    }
})();