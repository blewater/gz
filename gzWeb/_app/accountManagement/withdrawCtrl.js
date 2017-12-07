(function () {
    'use strict';
    var ctrlId = 'withdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', 'message', '$rootScope', 'accountManagement', 'iso4217', 'modals', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout, message, $rootScope, accountManagement, iso4217, modals) {
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
        //var moneyMatrixBankTransferFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixBankTransfer.html', ctrlId: 'withdrawMoneyMatrixBankTransferCtrl' }
        var moneyMatrixAdyenSepaFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixAdyenSepa.html', ctrlId: 'withdrawMoneyMatrixAdyenSepaCtrl' }
        //var withdrawFields = { templateUrl: '/_app/accountManagement/withdrawFields.html', ctrlId: 'withdrawFieldsCtrl' }
        var paymentMethodsFields = [];
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.VISA] = creditCardFields;
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.Maestro] = creditCardFields;
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill] = moneyMatrixSkrillFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixEnterCash] = moneyMatrixEnterCashFields;
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixBankTransfer] = moneyMatrixBankTransferFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixAdyenSepa] = moneyMatrixAdyenSepaFields;
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

        function sendTransactionReceipt(pid, appInsightsTrackEvent) {
            $timeout(function () {
                emBankingWithdraw.getTransactionInfo(pid).then(function (transactionResult) {
                    modals.receipt($scope.selected.method.displayName, transactionResult, true).then(function (response) {
                        $scope.waiting = false;
                        if (transactionResult.status === "success") {
                            appInsightsTrackEvent('TRANSACTION SUCCESS');
                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                            $scope.nsOk(true);
                        } else if (transactionResult.status === "incomplete") {
                            appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                            $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
                                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                            });
                        } else if (transactionResult.status === "pending") {
                            appInsightsTrackEvent('TRANSACTION PENDING');
                            $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
                                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                            });
                            $scope.setState(accountManagement.states.pendingWithdrawals);
                        } else if (transactionResult.status === "error") {
                            appInsightsTrackEvent('TRANSACTION ERROR');
                            init();
                        }
                    });
                }, function (error) {
                    $scope.waiting = false;
                    message.autoCloseError(error.desc);
                    init();
                });
            }, 2000);
        }

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
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent);
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
                                        sendTransactionReceipt(thirdPartyPromiseResult.$pid, appInsightsTrackEvent);
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