(function () {
    'use strict';
    var ctrlId = 'registerDepositCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', 'api', '$filter', 'message', 'constants', '$compile', '$controller', '$templateRequest', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, api, $filter, message, constants, $compile, $controller, $templateRequest, helpers) {
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
            //getGamingAccounts();
        };

        function getPaymentMethodCfg() {
            if (!$scope.paymentMethodCfg) {
                $scope.initializing = true;
                emBanking.getPaymentMethodCfg($scope.selectedMethod.code).then(function(paymentMethodCfgResult) {
                    $scope.paymentMethodCfg = paymentMethodCfgResult;
                    attachDepositFields($scope.paymentMethodCfg.paymentMethodCode);
                    $scope.initializing = false;
                }, function(error) {
                    console.log(error.desc);
                    $scope.initializing = false;
                });
            }
            else {
                attachDepositFields($scope.paymentMethodCfg.paymentMethodCode);
            }
        }

        function attachDepositFields(paymentMethodCode) {
            var depositFields = emBanking.getPaymentMethodFields(paymentMethodCode);
            var depositTemplateUrl = depositFields.templateUrl;
            var depositCtrlId = depositFields.ctrlId;
            $templateRequest(helpers.ui.getTemplate(depositTemplateUrl)).then(function (depositHtml) {
                var $depositFields = angular.element('#depositFields');
                $depositFields.html(depositHtml);
                var depositCtrl = $controller(depositCtrlId, { $scope: $scope });
                $depositFields.children().data('$ngControllerController', depositCtrl);
                $compile($depositFields.contents())($scope);
            });
        }

        function getGamingAccounts() {
            if (!$scope.gamingAccounts) {
                emBanking.getGamingAccounts(false, false).then(function (result) {
                    $scope.gamingAccounts = result.accounts;
                }, function (error) {
                    console.log(error.desc);
                });
            }
        }

        // #endregion

        // #region deposit
        $scope.submit = function () {
            if ($scope.form.$valid)
                deposit();
        };

        function deposit() {
            $scope.readFields().then(function(fields) {
                emBanking.prepare({
                    paymentMethodCode: $scope.selectedMethod,
                    fields: fields
                }).then(function(prepareResult) {
                    $scope.pid = prepareResult.pid;
                    if (prepareResult.status === "setup") {
                        // TODO: show confirmation page
                        var confirmPromise = message.modal("Please confirm...", {
                            nsSize: 'md',
                            nsTemplate: '/partials/messages/confirmDeposit.html',
                            //nsCtrl: 'confirmDepositCtrl',
                            nsParams: {
                                
                            },
                            nsStatic: true
                        });
                        confirmPromise.then(function () {
                            emBanking.confirm($scope.pid).then(function (confirmResult) {
                                if (confirmResult.status === "success") {
                                    emBanking.getTransactionInfo().then(function (transactionResult) {
                                        if (transactionResult.status === "success") {
                                            // TODO: show receipt page ...
                                            message.notify("You have made the deposit successfully!");
                                            emBanking.sendReceiptEmail(transactionResult.pid, "");
                                            $scope.nsOk(true);
                                        } else if (transactionResult.status === "incomplete") {
                                            // TODO: show transaction is not completed
                                        } else if (transactionResult.status === "pending") {
                                            // TODO: show transaction is pending
                                        } else if (transactionResult.status === "error") {
                                            // TODO: show error
                                        }
                                    }, function (error) {
                                        console.log(error.desc);
                                    });
                                } else if (confirmResult.status === "redirection") {
                                    // TODO: redirection ...
                                } else if (confirmResult.status === "instructions") {
                                    // TODO: instructions ...
                                } else {
                                    // TODO: log error ???
                                }
                            }, function (error) {
                                console.log(error.desc);
                            });
                        });
                    } else if (prepareResult.status === "redirection") {
                        // TODO: redirection ...
                    } else {
                        // TODO: log error ???
                        console.log("Unexpected payment method prepare status");
                    }
                }, function(error) {
                    console.log(error.desc);
                });
            }, function(error) {
                console.log(error.desc);
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