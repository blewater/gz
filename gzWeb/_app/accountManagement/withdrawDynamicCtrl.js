(function () {
    'use strict';
    var ctrlId = 'withdrawDynamicCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBankingWithdraw', 'helpers', '$timeout', 'message', '$rootScope', 'accountManagement', 'iso4217', 'modals', '$filter', ctrlFactory]);
    function ctrlFactory($scope, constants, emBankingWithdraw, helpers, $timeout, message, $rootScope, accountManagement, iso4217, modals, $filter) {
        // #region variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.selected = {
            group: undefined,
            method: undefined
        };
        $scope.model = {
            amount: undefined
        };
        // #endregion

        // #region payment methods fields
        var paymentMethodsFields = [];
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixCreditCard.html', ctrlId: 'withdrawMoneyMatrixCreditCardCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixTrustly] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixTrustly.html', ctrlId: 'withdrawMoneyMatrixTrustlyCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixSkrill] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixSkrill.html', ctrlId: 'withdrawMoneyMatrixSkrillCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixEnterCash] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixEnterCash.html', ctrlId: 'withdrawMoneyMatrixEnterCashCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixBankTransfer] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixBankTransfer.html', ctrlId: 'withdrawMoneyMatrixBankTransferCtrl' };
        paymentMethodsFields[emBankingWithdraw.PaymentMethodCode.MoneyMatrixAdyenSepa] = { templateUrl: '/_app/accountManagement/withdrawMoneyMatrixAdyenSepa.html', ctrlId: 'withdrawMoneyMatrixAdyenSepaCtrl' };
        function getPaymentMethodFields(paymentMethodCode) {
            return paymentMethodsFields[paymentMethodCode];
        };
        // #endregion

        // #region init
        function init() {
            $scope.initializing = true;

            $scope.selected.group = $scope.selectedMethodGroup;
            emBankingWithdraw.getPaymentMethodCfg($scope.selected.group[0].code, null).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;

                $scope.existingPayCards = $scope.paymentMethodCfg.fields.payCardID.options;
                $scope.maximumPayCards = $scope.paymentMethodCfg.fields.payCardID.maximumPayCards;
                $scope.thereAreExistingPayCards = $scope.existingPayCards.length > 0;
                $scope.canAddNewPayCard = $scope.existingPayCards.length < $scope.maximumPayCards;

                if ($scope.existingPayCards.length === 1) {
                    $scope.selected.method = $scope.existingPayCards[0];
                    $scope.onPayCardSelected($scope.selected.method);
                }

                $scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
                $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
                $scope.currency = $scope.gamingAccount.currency;
                $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
                $scope.accountLimitMax = Math.min($scope.accountLimits.max, $scope.gamingAccount.amount);
                $scope.limitMin = $scope.accountLimits.min;
                $scope.limitMax = $scope.accountLimitMax;
                var amountRange = " (between " + $filter('number')($scope.limitMin, 2) + " and " + $filter('number')($scope.limitMax, 2) + ")";
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
                if ($scope.limitMin < $scope.limitMax && !$rootScope.mobile)
                    $scope.amountPlaceholder += amountRange;

                if ($scope.paymentMethodCfg.monitoringScriptUrl) {
                    $.get($scope.paymentMethodCfg.monitoringScriptUrl, undefined, angular.noop, "script").fail(function () {
                        message.autoCloseError('Cannot load provided MonitoringScriptUrl');
                    });
                }

                if ($scope.paymentMethodCfg.fields.payCardID.registrationFields) {
                    $scope.registrationFields = [];
                    for (var regFieldKey in $scope.paymentMethodCfg.fields.payCardID.registrationFields) {
                        var regField = $scope.paymentMethodCfg.fields.payCardID.registrationFields[regFieldKey];
                        if (regField.mandatory) {
                            $scope.model[regFieldKey] = regField.defaultValue || null;
                            $scope.registrationFields.push({
                                key: regFieldKey,
                                label: regField.label || regFieldKey,
                                description: regField.description || regFieldKey,
                                type: regField.type,
                                regex: regField.regularExpression,
                                value: regField.defaultValue || null
                            });
                        }
                    }
                }

                $scope.initializing = false;
            }, function (error) {
                message.autoCloseError(error.desc);
                $scope.initializing = false;
            });
        };

        init();
        // #endregion

        // #region onPayCardSelected
        $scope.onPayCardSelected = function (payCardId) {
            var selectedPayCard = $filter('where')($scope.existingPayCards, { 'id': payCardId })[0];
            $scope.selected.method = selectedPayCard;
            //$scope.registrationFields = [];
            //for (var regFieldKey in $scope.paymentMethodCfg.fields.payCardID.registrationFields) {
            //    var regField = $scope.paymentMethodCfg.fields.payCardID.registrationFields[regFieldKey];
            //    $scope.registrationFields.push({
            //        key: regFieldKey,
            //        label: regField.label || regFieldKey,
            //        description: regField.description || regFieldKey,
            //        type: regField.type,
            //        regex: regField.regularExpression,
            //        value: regField.defaultValue || null
            //    });
            //    if (regField.mandatory) {
            //        $scope.model[regFieldKey] = regField.defaultValue || null;
            //    }
            //}
        };
        // #endregion

        // #region withdraw
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                withdraw();
        };

        function sendTransactionReceipt(pid, appInsightsTrackEvent) {
            var getTransactionInfoCall = function () { return emBankingWithdraw.getTransactionInfo(pid); };
            modals.receipt(getTransactionInfoCall, $scope.selected.method.displayName, true).then(function (transactionResult) {
                $scope.waiting = false;
                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                if (transactionResult.status === "success") {
                    appInsightsTrackEvent('TRANSACTION SUCCESS');
                    $scope.nsOk(true);
                } else if (transactionResult.status === "incomplete") {
                    appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                } else if (transactionResult.status === "pending") {
                    appInsightsTrackEvent('TRANSACTION PENDING');
                    $scope.setState(accountManagement.states.pendingWithdrawals);
                } else if (transactionResult.status === "error") {
                    appInsightsTrackEvent('TRANSACTION ERROR');
                }
                init();
            }, function (error) {
                $scope.waiting = false;
                message.autoCloseError(error.desc);
                init();
            });

            //$timeout(function () {
            //    emBankingWithdraw.getTransactionInfo(pid).then(function (transactionResult) {
            //        modals.receipt($scope.selected.method.displayName, transactionResult, true).then(function (response) {
            //            $scope.waiting = false;
            //            if (transactionResult.status === "success") {
            //                appInsightsTrackEvent('TRANSACTION SUCCESS');
            //                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
            //                $scope.nsOk(true);
            //            } else if (transactionResult.status === "incomplete") {
            //                appInsightsTrackEvent('TRANSACTION INCOMPLETE');
            //                $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
            //                    $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
            //                });
            //            } else if (transactionResult.status === "pending") {
            //                appInsightsTrackEvent('TRANSACTION PENDING');
            //                $rootScope.$on(constants.events.WITHDRAW_STATUS_CHANGED, function () {
            //                    $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
            //                });
            //                $scope.setState(accountManagement.states.pendingWithdrawals);
            //            } else if (transactionResult.status === "error") {
            //                appInsightsTrackEvent('TRANSACTION ERROR');
            //                init();
            //            }
            //        });
            //    }, function (error) {
            //        $scope.waiting = false;
            //        message.autoCloseError(error.desc);
            //        init();
            //    });
            //}, 2000);
        }

        function readFields() {
            var q = $q.defer();
            var fields = {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
            }
            if ($scope.paymentMethodCfg.monitoringScriptUrl)
                angular.extend(fields, { MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null });
            
            //if ($scope.selected.method)
            //    fields.payCardID = $scope.selected.method.payCard.id;
            //else if (angular.isFunction($scope.getRegistrationFields))
            //    angular.extend(fields, $scope.getRegistrationFields());

            q.resolve(fields);
            return q.promise;
        };

        function withdraw() {
            $scope.waiting = true;
            window.appInsights.trackEvent("WITHDRAW", { status: "READ FIELDS" });
            readFields().then(function (fields) {
                window.appInsights.trackEvent("WITHDRAW", { status: "PREPARE" });
                emBankingWithdraw.prepare($scope.selected.method.code, fields).then(function (prepareResult) {
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
                                    sendTransactionReceipt(confirmResult.pid, appInsightsTrackEvent);
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
                                        sendTransactionReceipt(thirdPartyPromiseResult.$pid, appInsightsTrackEvent);
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
                            nsBody: readConfirmMessage(prepareData),
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

        function readConfirmMessage(prepareData) {
            return "Do you want to withdraw the amount of " + prepareData.debitAmount + getConfirmMessageSuffix(prepareData) + "?";
        };
        function getConfirmMessageSuffix(prepareData) {
            switch ($scope.selected.method.code) {
                case emBankingWithdraw.PaymentMethodCode.MoneyMatrixCreditCard:
                    return " to " + prepareData.creditTo;
                default:
                    return " using " + $scope.selected.method.name;
            }
        };
    }
})();