﻿(function () {
    'use strict';
    var ctrlId = 'depositCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBanking', 'helpers', '$timeout', 'message', '$rootScope', '$location', '$log', 'iso4217', 'modals', ctrlFactory]);
    function ctrlFactory($scope, constants, emBanking, helpers, $timeout, message, $rootScope, $location, $log, iso4217, modals) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region payment methods fields
        //var creditCardFields = { templateUrl: '/_app/accountManagement/depositCreditCard.html', ctrlId: 'depositCreditCardCtrl' }
        var moneyMatrixCreditCardFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixCreditCard.html', ctrlId: 'depositMoneyMatrixCreditCardCtrl' }
        var moneyMatrixTrustlyFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixTrustly.html', ctrlId: 'depositMoneyMatrixTrustlyCtrl' }
        var moneyMatrixSkrillFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixSkrill.html', ctrlId: 'depositMoneyMatrixSkrillCtrl' }
        var moneyMatrixSkrill1TapFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixSkrill1Tap.html', ctrlId: 'depositMoneyMatrixSkrill1TapCtrl' }
        var moneyMatrixEnterCashFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixEnterCash.html', ctrlId: 'depositMoneyMatrixEnterCashCtrl' }
        //var moneyMatrixNetellerFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixNeteller.html', ctrlId: 'depositMoneyMatrixNetellerCtrl' }
        //var moneyMatrixPaySafeCardFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixPaySafeCard.html', ctrlId: 'depositMoneyMatrixPaySafeCardCtrl' }
        //var moneyMatrixEcoPayzFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixEcoPayz.html', ctrlId: 'depositMoneyMatrixEcoPayzCtrl' }
        var paymentMethodsFields = [];
        //paymentMethodsFields[emBanking.PaymentMethodCode.VISA] = creditCardFields;
        //paymentMethodsFields[emBanking.PaymentMethodCode.Maestro] = creditCardFields;
        //paymentMethodsFields[emBanking.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixSkrill] = moneyMatrixSkrillFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixSkrill1Tap] = moneyMatrixSkrill1TapFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixEnterCash] = moneyMatrixEnterCashFields;
        //paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixNeteller] = moneyMatrixNetellerFields;
        //paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixPaySafeCard] = moneyMatrixPaySafeCardFields;
        //paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixEcoPayz] = moneyMatrixEcoPayzFields;
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
            if ($scope.form.$valid) {
                deposit();
            }
        };

        function sendTransactionReceipt(pid, appInsightsTrackEvent) {
            emBanking.getTransactionInfo(pid).then(function (transactionResult) {
                modals.receipt($scope.selectedMethod.displayName, transactionResult).then(function (response) {
                    emBanking.sendReceiptEmail($scope.pid, "<div>" + response + "</div>");
                    $scope.waiting = false;
                    if (transactionResult.status === "success") {
                        appInsightsTrackEvent('TRANSACTION SUCCESS');
                        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                        $scope.nsOk(true);
                        //init();
                        if ($location.path() === constants.routes.home.path)
                            $location.path(constants.routes.games.path).search({});
                    } else if (transactionResult.status === "incomplete") {
                        appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                    } else if (transactionResult.status === "pending") {
                        appInsightsTrackEvent('TRANSACTION PENDING');
                        $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                        });
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
        }

        function deposit() {
            $scope.waiting = true;
            window.appInsights.trackEvent("DEPOSIT", { status: "READ FIELDS" });
            $scope.readFields().then(function (fields) {
                window.appInsights.trackEvent("DEPOSIT", { status: "PREPARE" });
                emBanking.prepare({ paymentMethodCode: $scope.selectedMethod.code, fields: fields }).then(function (prepareResult) {
                    $scope.pid = prepareResult.pid;

                    var prepareData = {
                        creditTo: prepareResult.credit.name,
                        creditAmount: iso4217.getCurrencyByCode(prepareResult.credit.currency).symbol + " " + prepareResult.credit.amount,
                        debitFrom: prepareResult.debit.name,
                        debitAmount: iso4217.getCurrencyByCode(prepareResult.debit.currency).symbol + " " + prepareResult.debit.amount
                    };

                    function appInsightsTrackEvent(status) {
                        window.appInsights.trackEvent("DEPOSIT", {
                            credit: prepareData.creditTo + " " + prepareData.creditAmount,
                            debit: prepareData.debitTo + " " + prepareData.debitAmount,
                            status: status
                        });
                    };

                    if (prepareResult.status === "setup") {
                        appInsightsTrackEvent('PREPARE SETUP');
                        message.confirm("Please confirm you want to continue with the deposit", function () {
                            emBanking.confirm($scope.pid).then(function (confirmResult) {
                                appInsightsTrackEvent('CONFIRM');
                                if (confirmResult.status === "success") {
                                    appInsightsTrackEvent('GET TRANSACTION INFO');
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent);
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
                                        sendTransactionReceipt(thirdPartyPromiseResult.$pid, appInsightsTrackEvent);
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