(function () {
    'use strict';
    var ctrlId = 'responsibleGamingCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emResponsibleGaming', '$filter', 'iso4217', 'message', '$rootScope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emResponsibleGaming, $filter, iso4217, message, $rootScope, $location, constants) {
        //var limitTypes = {
        //    deposit: 'deposit',
        //    depositPerDay: 'depositPerDay',
        //    depositPerWeek: 'depositPerWeek',
        //    depositPerMonth: 'depositPerMonth',
        //    wagering: 'wagering',
        //    loss: 'loss',
        //    session: 'session'
        //}

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
                ? (iso4217.getCurrencyByCode(limit.currency) ? iso4217.getCurrencyByCode(limit.currency).symbol : limit.currency) + " " + limit.amount + " / " + getAmountPeriod(limit.period)
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
        $scope.getExpiration = function (limit, queued) {
            return limit
                ? limit.expiryDate
                    ? (queued ? "from " : "until ") + $filter('date')(limit.expiryDate, $rootScope.xs ? 'short' : 'medium')
                    : angular.isUndefined(limit.expiryDate) ? "" : "No expiration"
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
                else
                    $location.path(constants.routes.home.path);
            });
        };
        function getLimits() {
            emResponsibleGaming.getLimits().then(function (response) {
                $scope.limits = response;
                initializeModel();
            });
        };
        function initializeModel() {
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
        };
        init();

        $scope.setDepositLimit = function () {
            emResponsibleGaming.setDepositLimit($scope.model.depositPeriod, $scope.model.depositAmount, $scope.currency).then(function () {
                getLimits();
            }, function (error) {
                message.error(error);
            });
        };
        $scope.removeDepositLimit = function () {
            message.confirm("You are about to remove " + $scope.limits.deposit.current.period + " deposit limit. Click OK to continue.", function () {
                emResponsibleGaming.removeDepositLimit($scope.limits.deposit.current.period).then(function () {
                    getLimits();
                }, function (error) {
                    message.error(error);
                });
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