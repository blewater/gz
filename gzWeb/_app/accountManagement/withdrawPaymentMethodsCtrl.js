﻿(function () {
    'use strict';
    var ctrlId = 'withdrawPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBankingWithdraw', 'message', 'accountManagement', '$filter', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBankingWithdraw, message, accountManagement, $filter) {
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
            //        message.autoCloseError(error.desc);
            //        $scope.initializing = false;
            //    });
            //}, function (error) {
            //    message.autoCloseError(error.desc);
            //    $scope.initializing = false;
            //});

            // TODO include all
            emBankingWithdraw.getSupportedPaymentMethods().then(function (paymentMethods) {
                for (var i = 0; i < paymentMethods.length; i++)
                    paymentMethods[i].displayName = emBankingWithdraw.getPaymentMethodDisplayName(paymentMethods[i]);
                $scope.groupedPaymentMethods = $filter('toArray')($filter('groupBy')(paymentMethods, 'name'));
                //$scope.paymentMethods = paymentMethods;
                //for (var i = 0; i < $scope.paymentMethods.length; i++)
                //    $scope.paymentMethods[i].displayName = emBankingWithdraw.getPaymentMethodDisplayName($scope.paymentMethods[i]);
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
        //$scope.selectPaymentMethod = function (method) {
        //    $scope.setState(accountManagement.states.withdraw, {
        //        paymentMethods: $scope.paymentMethods,
        //        selectedMethod: method
        //    });
        //};
        $scope.selectPaymentMethod = function (group) {
            $scope.setState(accountManagement.states.withdraw, {
                groupedPaymentMethods: $scope.groupedPaymentMethods,
                selectedMethodGroup: group
            });
        };
        // #endregion

    }
})();