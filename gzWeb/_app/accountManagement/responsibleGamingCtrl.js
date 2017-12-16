(function () {
    'use strict';
    var ctrlId = 'responsibleGamingCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emResponsibleGaming', '$filter', 'iso4217', 'message', '$rootScope', '$location', 'constants', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emResponsibleGaming, $filter, iso4217, message, $rootScope, $location, constants, $timeout) {
        $scope.periods = {
            daily: 'daily',
            weekly: 'weekly',
            monthly: 'monthly'
        }
        $scope.model = {
            depositPeriod: $scope.periods.daily,
            depositAmount: 0,
            wageringPeriod: $scope.periods.daily,
            wageringAmount: 0,
            lossPeriod: $scope.periods.daily,
            lossAmount: 0,
            sessionAmount: 0
        };

        $scope.getAmount = function (limit) {
            return limit
                ? getAmountText(limit.currency, limit.amount, limit.period)
                : "";
        }
        function getAmountPeriod(period) {
            switch (period) {
                case "daily": return "day";
                case "weekly": return "week";
                case "monthly": return "month";
                default: return "day";
            }
        }
        function getAmountText(currency, amount, period) {
            return (iso4217.getCurrencyByCode(currency) ? iso4217.getCurrencyByCode(currency).symbol : currency) + " " + amount + " / " + getAmountPeriod(period)
        }
        $scope.getExpiration = function (limit, queued) {
            return limit && limit.current
                ? limit.current.expiryDate
                    ? (queued ? "from " : "until ") + $filter('date')(moment.utc(limit.current.expiryDate).toDate(), $rootScope.xs ? 'short' : 'medium')
                    : angular.isUndefined(limit.current.expiryDate) ? "" : "No expiration"
                : "";
        };


        function init() {
            getCurrency();
            getLimits();
        };
        function getCurrency() {
            $scope.currency = "EUR";
            emWamp.getSessionInfo().then(function (sessionInfo) {
                if (sessionInfo.currency) {
                    $scope.currency = sessionInfo.currency;
                    $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount";
                }
                else {
                    $location.path(constants.routes.home.path);
                    $scope.nsCancel("User not logged in.");
                }
            });
        };
        function getLimits(callback) {
            emResponsibleGaming.getLimits().then(function (response) {
                $timeout(function () {
                    $scope.limits = response;
                    if ($scope.limits.deposit && $scope.limits.deposit.current) {
                        $scope.model.depositPeriod = $scope.limits.deposit.current.period;
                        $scope.model.depositAmount = $scope.limits.deposit.current.amount;
                    }
                    if ($scope.limits.wagering && $scope.limits.wagering.current) {
                        $scope.model.wageringPeriod = $scope.limits.wagering.current.period;
                        $scope.model.wageringAmount = $scope.limits.wagering.current.amount;
                    }
                    if ($scope.limits.loss && $scope.limits.loss.current) {
                        $scope.model.lossPeriod = $scope.limits.loss.current.period;
                        $scope.model.lossAmount = $scope.limits.loss.current.amount;
                    }
                    if ($scope.limits.session && $scope.limits.session.current) {
                        $scope.model.sessionAmount = $scope.limits.session.current.amount;
                    }

                    if (callback)
                        callback();
                }, 0);
            });
        };
        init();

        $scope.showInvestmentMsg = function() {
            message.info("Do you know that 50% of your losses are invested and can be tracked in the investment page?");
        }
        $scope.toggleEditDepositLimit = function () {
            $scope.editDeposit = !$scope.editDeposit;
        };
        $scope.setDepositLimit = function () {
            $rootScope.loading = true;
            emResponsibleGaming.setDepositLimit($scope.model.depositPeriod, $scope.model.depositAmount, $scope.currency).then(function () {
                getLimits(function () {
                    $rootScope.loading = false;
                    message.success("Your new deposit limit has been set to " + getAmountText($scope.currency, $scope.model.depositAmount, $scope.model.depositPeriod));
                    $scope.toggleEditDepositLimit();
                });
            }, function (error) {
                $rootScope.loading = false;
                message.error(error.desc);
            });
        };
        $scope.removeDepositLimit = function () {
            message.confirm("You are about to remove your " + $scope.limits.deposit.current.period + " deposit limit. Click OK to continue. ", function () {
                $rootScope.loading = true;
                emResponsibleGaming.removeDepositLimit($scope.limits.deposit.current.period).then(function () {
                    getLimits(function () {
                        $rootScope.loading = false;
                        message.success("Your " + $scope.limits.deposit.current.period + " deposit limit has been scheduled to remove.");
                        $scope.toggleEditDepositLimit();
                    });
                }, function (error) {
                    $rootScope.loading = false;
                    message.error(error.desc);
                });
            }, angular.noop, {
                nsClass: "warning",
                nsIconClass: 'fa-exclamation-triangle',
                nsIconClassInversed: true
            });
        };




        //function setMoneyLimit(period, amount, currency, func) {
        //};



        //function removeLimit (type, func) {
        //    message.confirm("You are about to remove " + type + " limit. Click OK to continue.", func);
        //};
        //$scope.removeDailytDepositLimit = function () {
        //    removeLimit("deposit (daily)", function () {
        //        emResponsibleGaming.removeDepositLimit({period: 'daily'})
        //    });
        //};
        //$scope.removeWeeklytDepositLimit = function () {
        //    removeLimit("deposit (weekly)", function () {
        //        emResponsibleGaming.removeDepositLimit({ period: 'weekly' })
        //    });
        //};
        //$scope.removeMonthlytDepositLimit = function () {
        //    removeLimit("deposit (monthly)", function () {
        //        emResponsibleGaming.removeDepositLimit({ period: 'monthly' })
        //    });
        //};
        //$scope.removeWageringLimit = function () {
        //    removeLimit("wagering", emResponsibleGaming.removeWageringLimit);
        //};
        //$scope.removeLossLimit = function () {
        //    removeLimit("loss", emResponsibleGaming.removeLossLimit);
        //};
        //$scope.removeSessionLimit = function () {
        //    removeLimit("session", emResponsibleGaming.removeSessionLimit);
        //};

    }

})();