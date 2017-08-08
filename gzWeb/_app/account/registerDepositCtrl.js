﻿(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', '$filter', 'message', 'constants', '$compile', '$controller', '$templateRequest', 'helpers', '$location', '$rootScope', '$timeout', '$log', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, $filter, message, constants, $compile, $controller, $templateRequest, helpers, $location, $rootScope, $timeout, $log, iso4217) {
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
                    message.error(error.desc);
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
            if ($scope.form.$valid)
                deposit();
        };

        function deposit() {
            $scope.waiting = true;
            $scope.readFields().then(function(fields) {
                emBanking.prepare({ paymentMethodCode: $scope.selectedMethod.code, fields: fields }).then(function(prepareResult) {
                    $scope.pid = prepareResult.pid;

                    var prepareData = {
                        creditTo: prepareResult.credit.name,
                        creditAmount: iso4217.getCurrencyByCode(prepareResult.credit.currency).symbol + " " + prepareResult.credit.amount,
                        debitFrom: prepareResult.debit.name,
                        debitAmount: iso4217.getCurrencyByCode(prepareResult.debit.currency).symbol + " " + prepareResult.debit.amount
                    };

                    if (prepareResult.status === "setup") {
                        message.confirm("Please confirm you want to continue with the deposit", function () {
                            emBanking.confirm($scope.pid).then(function (confirmResult) {
                                if (confirmResult.status === "success") {
                                    emBanking.getTransactionInfo(confirmResult.pid).then(function (transactionResult) {
                                        if (transactionResult.status === "success") {
                                            // TODO: show receipt page ...
                                            var msg = "You have made the deposit successfully!";
                                            message.success(msg, { nsType: 'toastr' });
                                            emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                            $scope.waiting = false;
                                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            $scope.nsOk(true);
                                            if ($location.path() === constants.routes.home.path)
                                                $location.path(constants.routes.games.path).search({});
                                        } else if (transactionResult.status === "incomplete") {
                                            $scope.waiting = false;
                                            $log.error("show transaction is not completed");
                                            // TODO: show transaction is not completed
                                        } else if (transactionResult.status === "pending") {
                                            $scope.waiting = false;
                                            $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
                                                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            });
                                        } else if (transactionResult.status === "error") {
                                            $scope.waiting = false;
                                            $log.error("show error");
                                            init();
                                            // TODO: show error
                                        }
                                    }, function (error) {
                                        $scope.waiting = false;
                                        message.error(error.desc);
                                        init();
                                    });
                                } else if (confirmResult.status === "redirection") {
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
                                        var msg = "You have made the deposit successfully!";
                                        message.success(msg, { nsType: 'toastr' });
                                        emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                        $scope.waiting = false;
                                        $scope.nsOk(true);
                                    }, function (thirdPartyPromiseError) {
                                        $scope.waiting = false;
                                        message.error(thirdPartyPromiseError);
                                        init();
                                    });
                                } else if (confirmResult.status === "instructions") {
                                    // TODO: instructions ...
                                } else {
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.error(error.desc);
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
                        // TODO: redirection ...
                    } else {
                        // TODO: log error ???
                        $scope.waiting = false;
                        message.error("Unexpected payment method prepare status");
                        init();
                    }
                }, function(error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                    init();
                });
            }, function(error) {
                $scope.waiting = false;
                message.error(error);
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