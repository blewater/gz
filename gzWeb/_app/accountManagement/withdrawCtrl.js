﻿(function () {
    'use strict';
    var ctrlId = 'withdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region payment methods fields
        var creditCardsFields = { templateUrl: '/_app/accountManagement/withdrawCreditCard.html', ctrlId: 'withdrawCreditCardCtrl' }
        var trustlyFields = { templateUrl: '/_app/accountManagement/withdrawTrustly.html', ctrlId: 'withdrawTrustlyCtrl' }
        var paymentMethodsFields = [];
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.VISA] = creditCardsFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.Maestro] = creditCardsFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MasterCard] = creditCardsFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.Trustly] = trustlyFields;
        function getPaymentMethodFields(paymentMethodCode) {
            return paymentMethodsFields[paymentMethodCode];
        };
        // #endregion

        // #region init
        function init() {
            getPaymentMethodCfg();
        };

        function getPaymentMethodCfg() {
            $scope.initializing = true;
            emBankingWithdraw.getPaymentMethodCfg($scope.selectedMethod.code, $scope.selectedMethod.payCard ? $scope.selectedMethod.payCard.id : null).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;
                attachFields($scope.paymentMethodCfg.paymentMethodCode);
                $scope.initializing = false;
            }, function (error) {
                message.error(error.desc);
                $scope.initializing = false;
            });

        }

        function attachFields(paymentMethodCode) {
            $timeout(function () {
                var paymentMethodFields = getPaymentMethodFields(paymentMethodCode);
                helpers.ui.compile({
                    selector: '#paymentMethodFields',
                    templateUrl: paymentMethodFields.templateUrl,
                    controllerId: paymentMethodFields.ctrlId,
                    scope: $scope
                });
            });
        }

        init();
        // #endregion

        // #region withdraw
        $scope.submit = function () {
            if ($scope.form.$valid)
                withdraw();
        };

        function withdraw() {
            $scope.waiting = true;
            $scope.readFields().then(function (fields) {
                emBankingWithdraw.prepare({
                    paymentMethodCode: $scope.selectedMethod.code,
                    fields: fields
                }).then(function (prepareResult) {
                    $scope.pid = prepareResult.pid;
                    if (prepareResult.status === "setup") {
                        // TODO: show confirmation page
                        var confirmPromise = message.modal("Please confirm you want to continue with the withdrawal", {
                            nsSize: 'md',
                            nsTemplate: '/partials/messages/confirmWithdraw.html',
                            //nsCtrl: 'confirmWithdrawCtrl',
                            nsParams: { fields: fields },
                            nsStatic: true
                        });
                        confirmPromise.then(function () {
                            emBankingWithdraw.confirm($scope.pid).then(function (confirmResult) {
                                if (confirmResult.status === "success") {
                                    emBankingWithdraw.getTransactionInfo(confirmResult.pid).then(function (transactionResult) {
                                        if (transactionResult.status === "success") {
                                            // TODO: show receipt page ...
                                            var msg = "You have made the withdrawal successfully!";
                                            message.success(msg, { nsType: 'toastr' });
                                            $scope.waiting = false;
                                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            $scope.nsOk(true);
                                            if ($location.path() === constants.routes.home.path)
                                                $location.path(constants.routes.games.path);
                                        } else if (transactionResult.status === "incomplete") {
                                            console.log("show transaction is not completed");
                                            // TODO: show transaction is not completed
                                        } else if (transactionResult.status === "pending") {
                                            console.log("show transaction is pending");
                                            // TODO: show transaction is pending
                                        } else if (transactionResult.status === "error") {
                                            console.log("show error");
                                            // TODO: show error
                                        }
                                    }, function (error) {
                                        message.error(error.desc);
                                    });
                                } else if (confirmResult.status === "redirection") {
                                    // TODO: redirection ...
                                } else if (confirmResult.status === "instructions") {
                                    // TODO: instructions ...
                                } else {
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.error(error.desc);
                            });
                        }, function () {
                            $scope.waiting = false;
                            $scope.paymentMethodCfg = undefined;
                            init();
                        });
                    } else if (prepareResult.status === "redirection") {
                        // TODO: redirection ...
                    } else {
                        // TODO: log error ???
                        $scope.waiting = false;
                        message.error("Unexpected payment method prepare status");
                    }
                }, function (error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                });
            }, function (error) {
                $scope.waiting = false;
                message.error(error.desc);
            });
        };
        // #endregion
    }
})();