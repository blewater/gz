﻿(function () {
    'use strict';
    var ctrlId = 'withdrawPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBankingWithdraw', 'message', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBankingWithdraw, message, accountManagement) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

        // #region init
        function loadPaymentMethods() {
            $scope.initializing = true;
            //emWamp.getSessionInfo().then(function (sessionInfo) {
            //    $scope.sessionInfo = sessionInfo;
            //    emBankingWithdraw.getSupportedPaymentMethods(sessionInfo.currency, true).then(function (paymentMethods) {
            //        $scope.paymentMethods = paymentMethods;
            //        for (var i = 0; i < $scope.paymentMethods.length; i++)
            //            $scope.paymentMethods[i].descr = getMethodDescr($scope.paymentMethods[i]);
            //        $scope.initializing = false;
            //    }, function (error) {
            //        message.error(error.desc);
            //        $scope.initializing = false;
            //    });
            //}, function (error) {
            //    message.error(error.desc);
            //    $scope.initializing = false;
            //});

            // TODO include all
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
        function getMethodDescr(method) {
            switch (method.code) {
                case emBankingWithdraw.PaymentMethodCode.VISA:
                case emBankingWithdraw.PaymentMethodCode.Maestro:
                case emBankingWithdraw.PaymentMethodCode.MasterCard:
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard:
                    return method.payCard.name;
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly:
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill:
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill1Tap:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixEnterCash:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixEuteller:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixNeteller:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixPaySafeCard:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixAdyenSepa:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixZimpler:
                //case emBankingWithdraw.PaymentMethodCode.MoneyMatrixEcoPayz:
                    return method.withdrawDesc || method.payCard.name;
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