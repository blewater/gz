(function () {
    'use strict';
    var ctrlId = 'registerSelectPaymentCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emWamp', 'api', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $filter, emWamp, api, message, constants) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        
        // #region steps
        $scope.currentStep = 2;
        $scope.steps = [
            { title: "Account Details" },
            { title: "Personal Details" },
            { title: "Deposit" }
        ];
        // #endregion

        // #region init
        function loadPaymentMethods() {
        }

        function init() {
            loadPaymentMethods();
        };
        // #endregion

        // #region register
        $scope.submit = function () {
            if ($scope.form.$valid)
                register();
        };
        
        function register() {
        };
        // #endregion

        // #region gotoGames
        $scope.gotoGames = function () {
            $scope.nsOk(false);
        };
        // #endregion

        init();
    }
})();