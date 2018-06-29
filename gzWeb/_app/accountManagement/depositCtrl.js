(function () {
    'use strict';
    var ctrlId = 'depositCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBanking', 'helpers', '$timeout', 'message', '$rootScope', '$location', '$log', 'api', 'iso4217', 'modals', '$filter', 'auth', ctrlFactory]);
    function ctrlFactory($scope, constants, emBanking, helpers, $timeout, message, $rootScope, $location, $log, api, iso4217, modals, $filter, auth) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region payment methods fields
        var paymentMethodsFields = [];
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixCreditCard] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixCreditCard.html', ctrlId: 'depositMoneyMatrixCreditCardCtrl' };
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixTrustly] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixTrustly.html', ctrlId: 'depositMoneyMatrixTrustlyCtrl' };
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixSkrill] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixSkrill.html', ctrlId: 'depositMoneyMatrixSkrillCtrl' };
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixSkrill1Tap] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixSkrill1Tap.html', ctrlId: 'depositMoneyMatrixSkrill1TapCtrl' };
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixEnterCash] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixEnterCash.html', ctrlId: 'depositMoneyMatrixEnterCashCtrl' };
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixBankTransfer] = { templateUrl: '/_app/accountManagement/depositMoneyMatrixBankTransfer.html', ctrlId: 'depositMoneyMatrixBankTransferCtrl' };
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
            emBanking.getPaymentMethodCfg($scope.selectedMethod.code).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;
                attachFields($scope.paymentMethodCfg.paymentMethodCode);
                $scope.initializing = false;
            }, function (error) {
                message.autoCloseError(error.desc);
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

        // #region deposit
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting) {
                deposit();
            }
        };

        function vrSendConvertion() {

            api.vrSendConversion(auth.data.email);

        }

        function sendTransactionReceipt(pid, appInsightsTrackEvent, logSuccessfulTransaction) {
            var getTransactionInfoCall = function () { return emBanking.getTransactionInfo(pid); };
            modals.receipt(getTransactionInfoCall, $scope.selectedMethod.displayName).then(function (transactionResult) {
                message.info("Do you know that 50% of your losses are invested and can be tracked in the investment page?");
                emBanking.sendReceiptEmail($scope.pid, "<div>" + transactionResult.status + "</div>");
                $scope.waiting = false;
                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                if (transactionResult.status === "success") {
                    appInsightsTrackEvent('TRANSACTION SUCCESS');
                    logSuccessfulTransaction();
                    $scope.nsOk(true);
                    if ($location.path() === constants.routes.home.path)
                        $location.path(constants.routes.games.path).search({});
                } else if (transactionResult.status === "incomplete") {
                    appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                    init();
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

        function deposit() {
            $scope.waiting = true;
            window.appInsights.trackEvent("DEPOSIT", { status: "READ FIELDS" });
            $scope.readFields().then(function (fields) {
                window.appInsights.trackEvent("DEPOSIT", { status: "PREPARE" });
                emBanking.prepare({ paymentMethodCode: $scope.selectedMethod.code, fields: fields }).then(function (prepareResult) {
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


                    function appInsightsTrackEvent(status, log) {
                        window.appInsights.trackEvent("DEPOSIT", {
                            credit: prepareData.creditTo + " " + prepareData.creditBaseAmount,
                            debit: prepareData.debitTo + " " + prepareData.debitBaseAmount,
                            status: status
                        });
                    };
                    function logSuccessfulTransaction() {
                        $log.info("SUCCESSFULL DEPOSIT: " + prepareData.debitBaseAmount);
                    };

                    if (prepareResult.status === "setup") {
                        appInsightsTrackEvent('PREPARE SETUP');
                        message.confirm("Please confirm you want to continue with the deposit", function () {
                            emBanking.confirm($scope.pid).then(function (confirmResult) {
                                appInsightsTrackEvent('CONFIRM');
                                if (confirmResult.status === "success") {
                                    appInsightsTrackEvent('GET TRANSACTION INFO');
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent, logSuccessfulTransaction);
                                } else if (confirmResult.status === "redirection") {
                                    appInsightsTrackEvent('CONFIRM REDIRECTION');
                                    var html = '<gz-third-party-iframe gz-redirection-form="redirectionForm"></gz-third-party-iframe>';
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
                                        init();
                                    });
                                } else if (confirmResult.status === "instructions") {
                                    appInsightsTrackEvent('CONFIRM INSTRUCTIONS');
                                    // TODO: instructions ...
                                } else {
                                    appInsightsTrackEvent('CONFIRM ERROR');
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.autoCloseError(error.desc);
                                appInsightsTrackEvent('CONFIRM FAILED');
                                init();
                            });
                        }, function () {
                            $scope.waiting = false;
                            $scope.paymentMethodCfg = undefined;
                            init();
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
                        init();
                    }
                }, function (error) {
                    $scope.waiting = false;
                    message.autoCloseError(error.desc);
                    window.appInsights.trackEvent("DEPOSIT", { status: "PREPARE ERROR" });
                    init();
                });
            }, function (error) {
                $scope.waiting = false;
                message.autoCloseError(error);
                window.appInsights.trackEvent("DEPOSIT", { status: "READ FIELDS ERROR" });
                init();
            });
        };
        // #endregion
    }
})();