(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', '$filter', 'message', 'constants', '$compile', '$controller', '$templateRequest', 'helpers', '$location', '$rootScope', '$timeout', '$log', 'iso4217', 'modals', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, $filter, message, constants, $compile, $controller, $templateRequest, helpers, $location, $rootScope, $timeout, $log, iso4217, modals) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.accountModel = $scope.accountModel || undefined;
        $scope.sessionInfo = $scope.sessionInfo || undefined;
        $scope.paymentMethods = $scope.paymentMethods || undefined;
        $scope.gamingAccounts = $scope.gamingAccounts || undefined;

        // #region steps
        $scope.currentStep = 2;
        $scope.steps = [
            { title: "Account Details" },
            { title: "Personal Details" },
            { title: "Deposit" }
        ];
        // #endregion

        // #region init
        function init() {
            getPaymentMethodCfg();
            window.appInsights.trackPageView("REGISTER DEPOSIT");
        };

        function getPaymentMethodCfg() {
            if (!$scope.paymentMethodCfg) {
                $scope.initializing = true;
                emBanking.getPaymentMethodCfg($scope.selectedMethod.code).then(function (paymentMethodCfgResult) {
                    $scope.paymentMethodCfg = paymentMethodCfgResult;
                    attachDepositFields($scope.paymentMethodCfg.paymentMethodCode);
                    $scope.initializing = false;
                }, function (error) {
                    message.autoCloseError(error.desc);
                    $scope.initializing = false;
                });
            }
            else {
                attachDepositFields($scope.paymentMethodCfg.paymentMethodCode);
            }
        }

        var creditCardFields = { templateUrl: '_app/account/registerDepositCreditCard.html', ctrlId: 'registerDepositCreditCardCtrl' }
        var moneyMatrixCreditCardFields = { templateUrl: '_app/account/registerDepositMoneyMatrixCreditCard.html', ctrlId: 'registerDepositMoneyMatrixCreditCardCtrl' }
        var moneyMatrixTrustlyFields = { templateUrl: '_app/account/registerDepositMoneyMatrixTrustly.html', ctrlId: 'registerDepositMoneyMatrixTrustlyCtrl' }
        var moneyMatrixSkrillFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixSkrill.html', ctrlId: 'registerDepositMoneyMatrixSkrillCtrl' }
        var moneyMatrixSkrill1TapFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixSkrill1Tap.html', ctrlId: 'registerDepositMoneyMatrixSkrill1TapCtrl' }
        var moneyMatrixEnterCashFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixEnterCash.html', ctrlId: 'registerDepositMoneyMatrixEnterCashCtrl' }
        //var moneyMatrixNetellerFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixNeteller.html', ctrlId: 'registerDepositMoneyMatrixNetellerCtrl' }
        //var moneyMatrixPaySafeCardFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixPaySafeCard.html', ctrlId: 'registerDepositMoneyMatrixPaySafeCardCtrl' }
        //var moneyMatrixEcoPayzFields = { templateUrl: '/_app/account/registerDepositMoneyMatrixEcoPayz.html', ctrlId: 'registerDepositMoneyMatrixEcoPayzCtrl' }
        var paymentMethodsFields = [];
        paymentMethodsFields[emBanking.PaymentMethodCode.VISA] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.Maestro] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MasterCard] = creditCardFields;
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

        function attachDepositFields(paymentMethodCode) {
            var depositFields = getPaymentMethodFields(paymentMethodCode);
            var depositTemplateUrl = depositFields.templateUrl;
            var depositCtrlId = depositFields.ctrlId;
            $templateRequest(helpers.ui.getTemplate(depositTemplateUrl)).then(function (depositHtml) {                
                var $depositFields = $('#depositFields');
                $depositFields.contents().remove();
                $depositFields.html(depositHtml);
                var depositCtrl = $controller(depositCtrlId, { $scope: $scope });
                $depositFields.children().data('$ngControllerController', depositCtrl);
                $compile($depositFields.contents())($scope);                    
            });
        }
        // #endregion

        // #region deposit
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                deposit();
        };

        function sendTransactionReceipt(pid, appInsightsTrackEvent, logSuccessfulTransaction) {
            var getTransactionInfoCall = function () { return emBanking.getTransactionInfo(pid); };
            modals.receipt(getTransactionInfoCall, $scope.selectedMethod.displayName).then(function (transactionResult) {
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

                    function appInsightsTrackEvent(status) {
                        window.appInsights.trackEvent("REGISTER DEPOSIT", {
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
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent, logSuccessfulTransaction);
                                    appInsightsTrackEvent('GET TRANSACTION INFO');
                                    //emBanking.getTransactionInfo(confirmResult.pid).then(function (transactionResult) {
                                    //    if (transactionResult.status === "success") {
                                    //        // TODO: show receipt page ...
                                    //        var msg = "You have made the deposit successfully!";
                                    //        message.success(msg, { nsType: 'toastr' });
                                    //        emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                    //        $scope.waiting = false;
                                    //        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                    //        $scope.nsOk(true);
                                    //        if ($location.path() === constants.routes.home.path)
                                    //            $location.path(constants.routes.games.path).search({});
                                    //    } else if (transactionResult.status === "incomplete") {
                                    //        $scope.waiting = false;
                                    //        $log.error("show transaction is not completed");
                                    //        // TODO: show transaction is not completed
                                    //    } else if (transactionResult.status === "pending") {
                                    //        $scope.waiting = false;
                                    //        $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
                                    //            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                    //        });
                                    //    } else if (transactionResult.status === "error") {
                                    //        $scope.waiting = false;
                                    //        $log.error("show error");
                                    //        init();
                                    //        // TODO: show error
                                    //    }
                                    //}, function (error) {
                                    //    $scope.waiting = false;
                                    //    message.autoCloseError(error.desc);
                                    //    init();
                                    //});
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
                                        //var msg = "You have made the deposit successfully!";
                                        //message.success(msg, { nsType: 'toastr' });
                                        //emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                        //$scope.waiting = false;
                                        //$scope.nsOk(true);
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
                }, function(error) {
                    $scope.waiting = false;
                    message.autoCloseError(error.desc);
                    window.appInsights.trackEvent("DEPOSIT", { status: "PREPARE ERROR" });
                    init();
                });
            }, function(error) {
                $scope.waiting = false;
                message.autoCloseError(error);
                window.appInsights.trackEvent("DEPOSIT", { status: "READ FIELDS ERROR" });
                init();
            });
        };
        // #endregion

        // #region backToPaymentMethods
        $scope.backToPaymentMethods = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/registerPaymentMethods.html',
                nsCtrl: 'registerPaymentMethodsCtrl',
                nsStatic: true,
                nsParams: {
                    accountModel: $scope.accountModel,
                    sessionInfo: $scope.sessionInfo,
                    paymentMethods: $scope.paymentMethods,
                    gamingAccounts: $scope.gamingAccounts
                }
            });
        };
        // #endregion

        init();
    }
})();