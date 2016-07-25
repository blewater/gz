(function () {
    'use strict';
    var ctrlId = 'withdrawPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBankingWithdraw', 'message', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBankingWithdraw, message) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

        $scope.accountModel = $scope.accountModel || undefined;
        $scope.sessionInfo = $scope.sessionInfo || undefined;
        $scope.paymentMethods = $scope.paymentMethods || undefined;
        $scope.gamingAccounts = $scope.gamingAccounts || undefined;

        // #region init
        function loadPaymentMethods() {
            if (!$scope.paymentMethods) {
                emBankingWithdraw.getPaymentMethods().then(function (response) {
                    $scope.paymentMethods = response.paymentMethods;
                    $scope.initializing = false;
                }, function (error) {
                    message.error(error.desc);
                    $scope.initializing = false;
                });
            }
        }

        function init() {
            loadPaymentMethods();
        };

        init();
        // #endregion

        // #region selectPaymentMethod
        $scope.selectPaymentMethod = function (method) {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerDeposit.html',
                nsCtrl: 'registerDepositCtrl',
                nsStatic: true,
                nsParams: {
                    accountModel: $scope.accountModel,
                    sessionInfo: $scope.sessionInfo,
                    paymentMethods: $scope.paymentMethods,
                    gamingAccounts: $scope.gamingAccounts,
                    selectedMethod: method
                }
            });
        };
        // #endregion

    }
})();