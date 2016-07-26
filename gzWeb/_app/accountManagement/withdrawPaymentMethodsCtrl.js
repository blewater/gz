(function () {
    'use strict';
    var ctrlId = 'withdrawPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBankingWithdraw', 'message', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBankingWithdraw, message, accountManagement) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

        // #region init
        function loadPaymentMethods() {
            $scope.initializing = true;
            emBankingWithdraw.getSupportedPaymentMethods().then(function (paymentMethods) {
                $scope.paymentMethods = paymentMethods;
                for (var i = 0; i < $scope.paymentMethods.length; i++)
                    $scope.paymentMethods[i].descr = getMethodDescr($scope.paymentMethods[i]);
                $scope.initializing = false;
            }, function (error) {
                message.error(error.desc);
                $scope.initializing = false;
            });
        }

        function init() {
            loadPaymentMethods();
        };

        init();
        // #endregion

        // #region methods
        //function getMethodDescr(method) {
        //    switch (method.code){
        //        case emBankingWithdraw.PaymentMethodCode.VISA:
        //        case emBankingWithdraw.PaymentMethodCode.Maestro:
        //        case emBankingWithdraw.PaymentMethodCode.MasterCard:
        //            return [method.withdrawDesc, method.payCard.name, method.payCard.cardExpiryDate, method.payCard.cardHolderName];
        //        case emBankingWithdraw.PaymentMethodCode.Trustly:
        //            return [method.withdrawDesc || method.code];
        //        default:
        //            return [method.code];
        //    }
        //};
        function getMethodDescr(method) {
            switch (method.code) {
                case emBankingWithdraw.PaymentMethodCode.VISA:
                case emBankingWithdraw.PaymentMethodCode.Maestro:
                case emBankingWithdraw.PaymentMethodCode.MasterCard:
                    return method.payCard.name;
                case emBankingWithdraw.PaymentMethodCode.Trustly:
                    return method.withdrawDesc || method.code;
                default:
                    return method.code;
            }
        };
        $scope.selectPaymentMethod = function (method) {
            $scope.setState(accountManagement.states.withdraw, {
                paymentMethods: $scope.paymentMethods,
                selectedMethod: method
            });
        };
        // #endregion

    }
})();