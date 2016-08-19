(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', '$filter', 'message', 'constants', '$compile', '$controller', '$templateRequest', 'helpers', '$location', '$rootScope', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, $filter, message, constants, $compile, $controller, $templateRequest, helpers, $location, $rootScope) {
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

        var creditCardFields = { templateUrl: '/partials/templates/registerDepositCreditCard.html', ctrlId: 'registerDepositCreditCardCtrl' }
        //var moneyMatrixCreditCardFields = { templateUrl: '/partials/templates/registerDepositMoneyMatrixCreditCard.html', ctrlId: 'registerDepositMoneyMatrixCreditCardCtrl' }
        var trustlyFields = { templateUrl: '/partials/templates/registerDepositTrustly.html', ctrlId: 'registerDepositTrustlyCtrl' }
        var paymentMethodsFields = [];
        paymentMethodsFields[emBanking.PaymentMethodCode.VISA] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.Maestro] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MasterCard] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixCreditCard] = creditCardFields;
        paymentMethodsFields[emBanking.PaymentMethodCode.MoneyMatrixTrustly] = trustlyFields;
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
                emBanking.prepare({
                    paymentMethodCode: $scope.selectedMethod.code,
                    fields: fields
                }).then(function(prepareResult) {
                    $scope.pid = prepareResult.pid;
                    if (prepareResult.status === "setup") {
                        // TODO: show confirmation page
                        var confirmPromise = message.modal("Please confirm you want to continue with the deposit", {
                            nsSize: 'md',
                            nsTemplate: '/partials/messages/confirmDeposit.html',
                            //nsCtrl: 'confirmDepositCtrl',
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
                                                $location.path(constants.routes.games.path);
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
                                    // TODO: redirection ...
                                } else if (confirmResult.status === "instructions") {
                                    // TODO: instructions ...
                                } else {
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                $scope.waiting = false;
                                message.error(error.desc);
                            });
                        }, function() {
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

        // #region backToPaymentMethods
        $scope.backToPaymentMethods = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerPaymentMethods.html',
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