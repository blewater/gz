(function () {
    'use strict';
    var ctrlId = 'withdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', 'message', '$rootScope', 'accountManagement', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout, message, $rootScope, accountManagement, iso4217) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region payment methods fields
        var creditCardFields = { templateUrl: '/_app/accountManagement/withdrawCreditCard.html', ctrlId: 'withdrawCreditCardCtrl' }
        var moneyMatrixCreditCardFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixCreditCard.html', ctrlId: 'withdrawMoneyMatrixCreditCardCtrl' }
        var moneyMatrixTrustlyFields = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixTrustly.html', ctrlId: 'withdrawMoneyMatrixTrustlyCtrl' }
        var paymentMethodsFields = [];
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.VISA] = creditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.Maestro] = creditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
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
            window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS" });
            $scope.readFields().then(function (fields) {
                window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE" });
                emBankingWithdraw.prepare($scope.selectedMethod.code, fields).then(function (prepareResult) {
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
                                            message.error("Transaction is not completed!");
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
                                            message.error(transactionResult.error);
                                        }
                                    }, function (error) {
                                        message.error(error.desc);
                                    });
                                } else if (confirmResult.status === "redirection") {
                                    appInsightsTrackEvent('CONFIRM REDIRECTION');
                                    var html = '<gz-third-party-iframe gz-redirection-form="redirectionForm"></gz-third-party-iframe>'
                                    var thirdPartyPromise = message.open({
                                        nsType: 'modal',
                                        nsSize: 'xl',
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
                                        $scope.nsOk(true);
                                    }, function (thirdPartyPromiseError) {
                                        appInsightsTrackEvent('TRANSACTION ERROR');
                                        $scope.waiting = false;
                                        message.error(thirdPartyPromiseError);
                                    });
                                } else {
                                    appInsightsTrackEvent('CONFIRM ERROR');
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.error(error.desc);
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
                        message.error("Unexpected payment method prepare status");
                        appInsightsTrackEvent('PREPARE FAILED');
                    }
                }, function (error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                    window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE ERROR" });
                });
            }, function (error) {
                $scope.waiting = false;
                message.error(error.desc);
                window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS ERROR" });
            });
        };
        // #endregion

        //$scope.getMethodName = function (method) {
        //    switch (method.code) {
        //        case emBankingWithdraw.PaymentMethodCode.VISA:
        //        case emBankingWithdraw.PaymentMethodCode.Maestro:
        //        case emBankingWithdraw.PaymentMethodCode.MasterCard:
        //        case emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard:
        //            return method.name;
        //        case emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly:
        //            return "Trustly";
        //        default:
        //            return method.name;
        //    }
        //};
    }
})();