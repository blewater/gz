﻿(function () {
    'use strict';
    var ctrlId = 'depositSelectedMethodCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emBanking', 'helpers', '$timeout', 'message', '$rootScope', '$location', '$log', 'iso4217', '$filter', ctrlFactory]);
    function ctrlFactory($scope, constants, emBanking, helpers, $timeout, message, $rootScope, $location, $log, iso4217, $filter) {
        // #region scope variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        // #endregion

        // #region init
        function init() {
            $scope.initializing = true;
            emBanking.getPaymentMethodCfg($scope.selectedMethod.code).then(function (paymentMethodCfgResult) {
                $scope.paymentMethodCfg = paymentMethodCfgResult;

                $scope.existingPayCards = $scope.paymentMethodCfg.fields.payCardID.options;
                $scope.maximumPayCards = $scope.paymentMethodCfg.fields.payCardID.maximumPayCards;
                $scope.thereAreExistingPayCards = $scope.existingPayCards.length > 0;
                $scope.canAddNewPayCard = $scope.existingPayCards.length < $scope.maximumPayCards;

                $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
                $scope.currency = $scope.gamingAccount.currency;
                $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount (between " + $scope.accountLimits.min + " and " + $scope.accountLimits.max + ")";

                $scope.model = {
                    selectedPayCard: undefined,
                    amount: undefined,
                    bonusCode: undefined
                };

                customInit($scope.selectedMethod);

                $scope.initializing = false;
            }, function (error) {
                message.error(error.desc);
                $scope.initializing = false;
            });
        };

        function customInit(method) {
            if (method.code === emBanking.PaymentMethodCode.MoneyMatrixTrustly)
                method.displayName = "Trustly";
            else if (method.code === emBanking.PaymentMethodCode.MoneyMatrixEnterCash)
                method.displayName = "EnterCash";
            else 
                method.displayName = method.name;

            if (method.code === emBanking.PaymentMethodCode.MoneyMatrixSkrill)
                $scope.model.payCardName = undefined;

            switch (method.code) {
                case emBanking.PaymentMethodCode.VISA:
                    break;
                case emBanking.PaymentMethodCode.Maestro:
                    break;
                case emBanking.PaymentMethodCode.MasterCard:
                    break;
                case emBanking.PaymentMethodCode.MoneyMatrixCreditCard:
                    break;
                case emBanking.PaymentMethodCode.MoneyMatrixSkrill:
                    break;
                case emBanking.PaymentMethodCode.MoneyMatrixSkrill1Tap:
                    break;
                case emBanking.PaymentMethodCode.MoneyMatrixTrustly:
                    method.displayName = "Trustly";
                    break;
                case emBanking.PaymentMethodCode.MoneyMatrixEnterCash:
                    method.displayName = "EnterCash";
                    break;
                default:
                    $scope.model.payCardName = undefined;
                    method.displayName = method.name;
                    break;
            }
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
                                    emBanking.getTransactionInfo(confirmResult.pid).then(function (transactionResult) {
                                        if (transactionResult.status === "success") {
                                            appInsightsTrackEvent('TRANSACTION SUCCESS');
                                            var msg = "You have made the deposit successfully!";
                                            message.success(msg, { nsType: 'toastr' });
                                            emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                            $scope.waiting = false;
                                            $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            $scope.nsOk(true);
                                            if ($location.path() === constants.routes.home.path)
                                                $location.path(constants.routes.games.path).search({});
                                        } else if (transactionResult.status === "incomplete") {
                                            appInsightsTrackEvent('TRANSACTION INCOMPLETE');
                                            $scope.waiting = false;
                                            $log.error("show transaction is not completed");
                                            // TODO: show transaction is not completed
                                        } else if (transactionResult.status === "pending") {
                                            appInsightsTrackEvent('TRANSACTION PENDING');
                                            $scope.waiting = false;
                                            $rootScope.$on(constants.events.DEPOSIT_STATUS_CHANGED, function () {
                                                $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                                            });
                                        } else if (transactionResult.status === "error") {
                                            appInsightsTrackEvent('TRANSACTION ERROR');
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
                                        appInsightsTrackEvent('TRANSACTION SUCCESS');
                                        var msg = "You have made the deposit successfully!";
                                        message.success(msg, { nsType: 'toastr' });
                                        emBanking.sendReceiptEmail($scope.pid, "<div>" + msg + "</div>");
                                        $scope.waiting = false;
                                        $scope.nsOk(true);
                                    }, function (thirdPartyPromiseError) {
                                        appInsightsTrackEvent('TRANSACTION ERROR');
                                        $scope.waiting = false;
                                        message.error(thirdPartyPromiseError);
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
                                message.error(error.desc);
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
                        message.error("Unexpected payment method prepare status");
                        appInsightsTrackEvent('PREPARE FAILED');
                        init();
                    }
                }, function (error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                    window.appInsights.trackEvent("DEPOSIT", { status: "PREPARE ERROR" });
                    init();
                });
            }, function (error) {
                $scope.waiting = false;
                message.error(error);
                window.appInsights.trackEvent("DEPOSIT", { status: "READ FIELDS ERROR" });
                init();
            });
        };
        // #endregion
    }
})();