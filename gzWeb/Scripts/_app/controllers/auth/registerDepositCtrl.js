(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', 'api', '$filter', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, api, $filter, message, constants) {
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
        function init() {
        };
        // #endregion

        // #region deposit
        $scope.submit = function () {
            if ($scope.form.$valid)
                deposit();
        };

        function deposit() {
            $scope.nsOk(true);
        };
        // #endregion

        // #region backToPaymentMethods
        $scope.backToPaymentMethods = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerPaymentMethods.html',
                nsCtrl: 'registerPaymentMethodsCtrl',
                nsStatic: true,
                nsParams: { accountModel: $scope.accountModel }
            });
        };
        // #endregion

        init();
    }
})();