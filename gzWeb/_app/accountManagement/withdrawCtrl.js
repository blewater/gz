﻿(function () {
    'use strict';
    var ctrlId = 'withdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', 'message', '$rootScope', 'accountManagement', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout, message, $rootScope, accountManagement, iso4217) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.selected = {
            group: undefined,
            method: undefined
        };
        // #endregion

        // #region payment methods fields
        //var creditCardFields = { templateUrl: '/_app/accountManagement/withdrawCreditCard.html', ctrlId: 'withdrawCreditCardCtrl' }
        var moneyMatrixCreditCardFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixCreditCard.html', ctrlId: 'withdrawMoneyMatrixCreditCardCtrl' }
        var moneyMatrixTrustlyFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixTrustly.html', ctrlId: 'withdrawMoneyMatrixTrustlyCtrl' }
        var moneyMatrixSkrillFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixSkrill.html', ctrlId: 'withdrawMoneyMatrixSkrillCtrl' }
        var moneyMatrixEnterCashFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixEnterCash.html', ctrlId: 'withdrawMoneyMatrixEnterCashCtrl' }
        //var withdrawFields = { templateUrl: '/_app/accountManagement/withdrawFields.html', ctrlId: 'withdrawFieldsCtrl' }
        var paymentMethodsFields = [];
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.VISA] = creditCardFields;
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.Maestro] = creditCardFields;
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill] = moneyMatrixSkrillFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixEnterCash] = moneyMatrixEnterCashFields;
        function getPaymentMethodFields(paymentMethodCode) {
            return paymentMethodsFields[paymentMethodCode];
        };
        // #endregion

        // #region init
        function init() {
            $scope.selected.group = $scope.selectedMethodGroup;
            if ($scope.selected.group.length === 1) {
                $scope.selected.method = $scope.selected.group[0];
                getPaymentMethodCfg();
            }
        };

        $scope.onPayCardSelected = function () {
            if ($scope.selected.method)
                getPaymentMethodCfg();
            else
                angular.element('#withdrawFields').contents().remove();
        };

        function getPaymentMethodCfg() {
            $scope.initializing = true;
            emBankingWithdraw.getPaymentMethodCfg($scope.selected.method.code, $scope.selected.method.payCard ? $scope.selected.method.payCard.id : null).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;
                attachFields($scope.paymentMethodCfg.paymentMethodCode);
                $scope.initializing = false;
            }, function (error) {
                message.autoCloseError(error.desc);
                $scope.initializing = false;
            });
        }

        function attachFields(paymentMethodCode) {
            var paymentMethodFields = getPaymentMethodFields(paymentMethodCode);
            helpers.ui.compile({
                selector: '#withdrawFields',
                templateUrl: paymentMethodFields.templateUrl,
                controllerId: paymentMethodFields.ctrlId,
                scope: $scope
            });
        }

        init();
        // #endregion

        // #region withdraw
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                withdraw();
        };

        //function sendTransactionReceipt(pid, appInsightsTrackEvent) {
        //    emBanking.getTransactionInfo(pid).then(function (transactionResult) {
        //        modals.receipt($scope.selectedMethod.displayName, transactionResult, true).then(function (response) {
        //            emBanking.sendReceiptEmail($scope.pid, "<div>" + response + "</div>");
        //            $scope.waiting = false;
        //            if (transactionResult.status === "success") {
        //                appInsightsTrackEvent('TRANSACTION SUCCESS');
        //                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
        //                $scope.nsOk(true);
        //                //init();
        //                if ($location.path() === constants.routes.home.path)
        //                    $location.path(constants.routes.games.path).search({});
        //            } else if (transactionResult.status === "incomplete") {
        //                appInsightsTrackEvent('TRANSACTION INCOMPLETE');
        //            } else if (transactionResult.status === "pending") {
        //                appInsightsTrackEvent('TRANSACTION PENDING');
        //                $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
        //                    $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
        //                });
        //            } else if (transactionResult.status === "error") {
        //                appInsightsTrackEvent('TRANSACTION ERROR');
        //                init();
        //            }
        //        });
        //    }, function (error) {
        //        $scope.waiting = false;
        //        message.autoCloseError(error.desc);
        //        init();
        //    });
        //}

        function withdraw() {
            $scope.waiting = true;
            window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS" });
            $scope.readFields().then(function (fields) {
                window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE" });
                emBankingWithdraw.prepare($scope.selected.method.code, fields).then(function (prepareResult) {
                    $scope.pid = prepareResult.pid;

                    var prepareData = {
                        creditTo: prepareResult.credit.name,
                        creditAmount: iso4217.getCurrencyByCode(prepareResult.credit.currency).symbol + " " + prepareResult.credit.amount,
                        debitFrom: prepareResult.debit.name,
                        debitAmount: iso4217.getCurrencyByCode(prepareResult.debit.currency).symbol + " " + prepareResult.debit.amount
                    };

                    function appInsightsTrackEvent(status) {
                        window.appInsights.trackEvent("WITHDRAW", {
                            credit: prepareData.creditTo + " " + prepareData.creditAmount,
                            debit: prepareData.debitTo + " " + prepareData.debitAmount,
                            status: status
                        });
                    };

                    if (prepareResult.status === "setup") {
                        appInsightsTrackEvent('PREPARE SETUP');
                        message.confirm("Please confirm you want to continue with the withdrawal", function () {
                            emBankingWithdraw.confirm($scope.pid).then(function (confirmResult) {
                                appInsightsTrackEvent('CONFIRM');
                                if (confirmResult.status === "setup") {
                                    appInsightsTrackEvent('GET TRANSACTION INFO');
                                    emBankingWithdraw.getTransactionInfo(confirmResult.pid).then(function (transactionResult) {
                                        if (transactionResult.status === "success") {
                                            appInsightsTrackEvent('TRANSACTION SUCCESS');
                                            var msg = "Withdrawal completed successfully at " + transactionResult.time + "!";
                                            message.success(msg, { nsType: 'toastr' });
                                            $scope.waiting = false;
                                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            $scope.nsOk(true);
                                        } else if (transactionResult.status === "incomplete") {
                                            appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                                            $scope.waiting = false;
                                            message.autoCloseError("Transaction is not completed!");
                                        } else if (transactionResult.status === "pending") {
                                            appInsightsTrackEvent('TRANSACTION PENDING');
                                            $scope.waiting = false;
                                            $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
                                                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            });
                                            $scope.setState(accountManagement.states.pendingWithdrawals);
                                        } else if (transactionResult.status === "error") {
                                            appInsightsTrackEvent('TRANSACTION ERROR');
                                            $scope.waiting = false;
                                            message.autoCloseError(transactionResult.error);
                                        }
                                    }, function (error) {
                                        message.autoCloseError(error.desc);
                                    });
                                } else if (confirmResult.status === "redirection") {
                                    appInsightsTrackEvent('CONFIRM REDIRECTION');
                                    var html = '<gz-third-party-iframe gz-redirection-form="redirectionForm"></gz-third-party-iframe>'
                                    var thirdPartyPromise = message.open({
                                        nsType: 'modal',
                                        nsSize: 'auto',
                                        nsBody: html,
                                        nsStatic: true,
                                        nsParams: {
                                            redirectionForm: confirmResult.redirectionForm
                                        },
                                        nsShowClose: false
                                    });
                                    thirdPartyPromise.then(function (thirdPartyPromiseResult) {
                                        appInsightsTrackEvent('TRANSACTION SUCCESS');
                                        var msg = "You have made the withdrawal successfully!";
                                        message.success(msg, { nsType: 'toastr' });
                                        $scope.waiting = false;
                                        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                        $scope.nsOk(true);
                                    }, function (thirdPartyPromiseError) {
                                        appInsightsTrackEvent('TRANSACTION ERROR');
                                        $scope.waiting = false;
                                        message.autoCloseError(thirdPartyPromiseError);
                                    });
                                } else {
                                    appInsightsTrackEvent('CONFIRM ERROR');
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.autoCloseError(error.desc);
                                appInsightsTrackEvent('CONFIRM FAILED');
                            });
                        }, function () {
                            $scope.waiting = false;
                            //$scope.paymentMethodCfg = undefined;
                            //init();
                        }, {
                            nsBody: $scope.readConfirmMessage(prepareData),
                            nsStatic: true,
                            nsSize: 'md'
                        });
                    } else if (prepareResult.status === "redirection") {
                        appInsightsTrackEvent('PREPARE REDIRECTION');
                        // TODO: redirection ...
                    } else {
                        // TODO: log error ???
                        $scope.waiting = false;
                        message.autoCloseError("Unexpected payment method prepare status");
                        appInsightsTrackEvent('PREPARE FAILED');
                    }
                }, function (error) {
                    $scope.waiting = false;
                    message.autoCloseError(error.desc);
                    window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE ERROR" });
                });
            }, function (error) {
                $scope.waiting = false;
                message.autoCloseError(error.desc);
                window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS ERROR" });
            });
        };
        // #endregion

        $scope.readConfirmMessage = function (prepareData) {
            return "Do you want to withdraw the amount of " + prepareData.debitAmount + getConfirmMessageSuffix(prepareData) + "?";
        };
        function getConfirmMessageSuffix(prepareData) {
            switch ($scope.selected.method.code) {
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard:
                    return " to " + prepareData.creditTo;
                default:
                    return " using " + $scope.selected.method.name;
            }
        };
    }
})();