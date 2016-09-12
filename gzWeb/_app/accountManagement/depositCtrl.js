(function () {
    'use strict';
    var ctrlId = 'depositCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBanking', 'helpers', '$timeout', 'message', '$rootScope', '$location', ctrlFactory]);
    function ctrlFactory($scope, constants, emBanking, helpers, $timeout, message, $rootScope, $location) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region payment methods fields
        var creditCardFields = { templateUrl: '/_app/accountManagement/depositCreditCard.html', ctrlId: 'depositCreditCardCtrl' }
        var moneyMatrixCreditCardFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixCreditCard.html', ctrlId: 'depositMoneyMatrixCreditCardCtrl' }
        var moneyMatrixTrustlyFields = { templateUrl: '/_app/accountManagement/depositMoneyMatrixTrustly.html', ctrlId: 'depositMoneyMatrixTrustlyCtrl' }
        var paymentMethodsFields = [];
        paymentMethodsFields[emBanking.PaymentMethodCode.VISA] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.Maestro] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixCreditCard] = moneyMatrixCreditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixTrustly] = moneyMatrixTrustlyFields;
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

        // #region deposit
        $scope.submit = function () {
            if ($scope.form.$valid) {
                deposit();
            }
        };

        function deposit() {
            $scope.waiting = true;
            $scope.readFields().then(function (fields) {
                emBanking.prepare({paymentMethodCode: $scope.selectedMethod.code, fields: fields}).then(function (prepareResult) {
                    $scope.pid = prepareResult.pid;
                    if (prepareResult.status === "setup") {
                        // TODO: show confirmation page
                        var confirmPromise = message.modal("Please confirm you want to continue with the deposit", {
                            nsSize: 'md',
                            nsTemplate: '_app/account/confirmDeposit.html',
                            nsCtrl: 'confirmDepositCtrl',
                            nsParams: { fields: fields },
                            nsStatic: true
                        });
                        confirmPromise.then(function () {
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
                message.error(error);
            });
        };
        // #endregion

    }
})();