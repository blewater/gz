(function () {
    'use strict';
    var ctrlId = 'withdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', 'message', '$rootScope', 'accountManagement', 'iso4217', 'modals', '$filter', '$log', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout, message, $rootScope, accountManagement, iso4217, modals, $filter, $log) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.selected = {
            group: undefined,
            method: undefined
        };
        // #endregion

        // #region payment methods fields
        var paymentMethodsFields = [];
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixCreditCard.html', ctrlId: 'withdrawMoneyMatrixCreditCardCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixTrustly.html', ctrlId: 'withdrawMoneyMatrixTrustlyCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixSkrill.html', ctrlId: 'withdrawMoneyMatrixSkrillCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixEnterCash] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixEnterCash.html', ctrlId: 'withdrawMoneyMatrixEnterCashCtrl' };
        //paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixBankTransfer] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixBankTransfer.html', ctrlId: 'withdrawMoneyMatrixBankTransferCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixAdyenSepa] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixAdyenSepa.html', ctrlId: 'withdrawMoneyMatrixAdyenSepaCtrl' };
        function getPaymentMethodFields(paymentMethodCode) {
            return paymentMethodsFields[paymentMethodCode];
        };
        // #endregion

        // #region init
        function init() {
            $scope.selected.group = $scope.selectedMethodGroup;
            getPaymentMethodCfg();
        };

        function getPaymentMethodCfg() {
            $scope.initializing = true;
            //var payCardId = $scope.selected.method.payCard ? $scope.selected.method.payCard.id : null;
            emBankingWithdraw.getPaymentMethodCfg($scope.selected.group[0].code, null).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;

                $scope.existingPayCards = $scope.paymentMethodCfg.fields.payCardID.options;
                $scope.maximumPayCards = $scope.paymentMethodCfg.fields.payCardID.maximumPayCards;
                $scope.thereAreExistingPayCards = $scope.existingPayCards.length > 0;
                $scope.canAddNewPayCard = $scope.existingPayCards.length < $scope.maximumPayCards;

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

        function sendTransactionReceipt(pid, appInsightsTrackEvent, logSuccessfulTransaction) {
            var getTransactionInfoCall = function () { return emBankingWithdraw.getTransactionInfo(pid); };
            modals.receipt(getTransactionInfoCall, $scope.selected.group[0].displayName, true).then(function (transactionResult) {
                $scope.waiting = false;
                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                if (transactionResult.status === "success") {
                    appInsightsTrackEvent('TRANSACTION SUCCESS');
                    logSuccessfulTransaction();
                    $scope.nsOk(true);
                    init();
                } else if (transactionResult.status === "incomplete") {
                    appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                    init();
                } else if (transactionResult.status === "pending") {
                    appInsightsTrackEvent('TRANSACTION PENDING');
                    $scope.setState(accountManagement.states.pendingWithdrawals);
                } else if (transactionResult.status === "error") {
                    appInsightsTrackEvent('TRANSACTION ERROR');
                    init();
                }
            }, function (error) {
                $scope.waiting = false;
                message.autoCloseError(error.desc);
                init();
            });
        }

        function withdraw() {
            $scope.waiting = true;
            window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS" });
            $scope.readFields().then(function (fields) {
                window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE" });
                emBankingWithdraw.prepare($scope.selected.group[0].code, fields).then(function (prepareResult) {
                    $scope.pid = prepareResult.pid;

                    var rates = $scope.paymentMethodCfg.fields.currency.rates;
                    var baseCurrencyRate = rates[constants.baseCurrency];
                    var creditRate = rates[prepareResult.credit.currency];
                    var debitRate = rates[prepareResult.debit.currency];
                    var baseCurrencyCredit = $filter('number')(prepareResult.credit.amount * baseCurrencyRate / creditRate, 1);
                    var baseCurrencyDebit = $filter('number')(prepareResult.debit.amount * baseCurrencyRate / debitRate, 1);

                    var prepareData = {
                        creditTo: prepareResult.credit.name,
                        creditAmount: iso4217.getCurrencyByCode(prepareResult.credit.currency).symbol + " " + prepareResult.credit.amount,
                        creditBaseAmount: iso4217.getCurrencyByCode(constants.baseCurrency).symbol + " " + baseCurrencyCredit,
                        debitFrom: prepareResult.debit.name,
                        debitAmount: iso4217.getCurrencyByCode(prepareResult.debit.currency).symbol + " " + prepareResult.debit.amount,
                        debitBaseAmount: iso4217.getCurrencyByCode(constants.baseCurrency).symbol + " " + baseCurrencyDebit,
                    };

                    function appInsightsTrackEvent(status) {
                        window.appInsights.trackEvent("WITHDRAW", {
                            credit: prepareData.creditTo + " " + prepareData.creditBaseAmount,
                            debit: prepareData.debitTo + " " + prepareData.debitBaseAmount,
                            status: status
                        });
                    };
                    function logSuccessfulTransaction() {
                        $log.info("SUCCESSFULL WITHDRAWAL: " + prepareData.creditBaseAmount);
                    };

                    if (prepareResult.status === "setup") {
                        appInsightsTrackEvent('PREPARE SETUP');
                        message.confirm("Please confirm you want to continue with the withdrawal", function () {
                            emBankingWithdraw.confirm($scope.pid).then(function (confirmResult) {
                                appInsightsTrackEvent('CONFIRM');
                                if (confirmResult.status === "setup") {
                                    appInsightsTrackEvent('GET TRANSACTION INFO');
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent, logSuccessfulTransaction);
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
                                        sendTransactionReceipt(thirdPartyPromiseResult.$pid, appInsightsTrackEvent, logSuccessfulTransaction);
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
            switch ($scope.selected.group[0].code) {
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard:
                    return " to " + prepareData.creditTo;
                default:
                    return " using " + $scope.selected.group[0].name;
            }
        };
    }
})();