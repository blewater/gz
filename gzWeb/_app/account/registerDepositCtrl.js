(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', '$filter', 'message', 'constants', '$compile', '$controller', '$templateRequest', 'helpers', '$location', '$rootScope', '$timeout', '$log', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, $filter, message, constants, $compile, $controller, $templateRequest, helpers, $location, $rootScope, $timeout, $log) {
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
        var paymentMethodsFields = [];
        paymentMethodsFields[emBanking.PaymentMethodCode.VISA] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.Maestro] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
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
                                            $log.error("show transaction is not completed");
                                            // TODO: show transaction is not completed
                                        } else if (transactionResult.status === "pending") {
                                            $log.error("show transaction is pending");
                                            // TODO: show transaction is pending
                                        } else if (transactionResult.status === "error") {
                                            $log.error("show error");
                                            // TODO: show error
                                        }
                                    }, function (error) {
                                        message.error(error.desc);
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
                                        $timeout(function () {
                                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                        }, 1000);
                                        $scope.nsOk(true);
                                    }, function (thirdPartyPromiseError) {
                                        $scope.waiting = false;
                                        message.error(thirdPartyPromiseError);
                                    });
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
                    }
                }, function(error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                });
            }, function(error) {
                $scope.waiting = false;
                message.error(error.desc);
            });
        };
        // #endregion

        $scope.getMethodName = function (method) {
            switch (method.code) {
                case emBanking.PaymentMethodCode.VISA:
                case emBanking.PaymentMethodCode.Maestro:
                case emBanking.PaymentMethodCode.MasterCard:
                case emBanking.PaymentMethodCode.MoneyMatrixCreditCard:
                    return method.name;
                case emBanking.PaymentMethodCode.MoneyMatrixTrustly:
                    return "Trustly";
                default:
                    return method.name;
            }
        };

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