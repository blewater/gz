(function () {
    'use strict';
    var ctrlId = 'withdrawPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBankingWithdraw', 'message', 'accountManagement', '$filter', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBankingWithdraw, message, accountManagement, $filter) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

        // #region init
        function loadPaymentMethods() {
            $scope.initializing = true;
            emBankingWithdraw.getSupportedPaymentMethods().then(function (paymentMethods) {
                for (var i = 0; i < paymentMethods.length; i++)
                    paymentMethods[i].displayName = emBankingWithdraw.getPaymentMethodDisplayName(paymentMethods[i]);
                $scope.paymentMethods = paymentMethods;
                $scope.groupedPaymentMethods = $filter('toArray')($filter('groupBy')(paymentMethods, 'name'));
                $scope.initializing = false;
            }, function (error) {
                message.autoCloseError(error.desc);
                $scope.initializing = false;
            });
        }

        function init() {
            loadPaymentMethods();
        };

        init();
        // #endregion

        // #region methods
        $scope.selectPaymentMethod = function (group) {
            $scope.setState(accountManagement.states.withdraw, {
                paymentMethods: $scope.paymentMethods,
                //selectedMethod: method
                groupedPaymentMethods: $scope.groupedPaymentMethods,
                selectedMethodGroup: group
            });
        };
        // #endregion
    }
})();