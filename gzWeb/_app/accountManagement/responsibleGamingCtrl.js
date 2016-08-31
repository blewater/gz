(function () {
    'use strict';
    var ctrlId = 'responsibleGamingCtrl';
    APP.controller(ctrlId, ['$scope', 'emResponsibleGaming', '$filter', ctrlFactory]);
    function ctrlFactory($scope, emResponsibleGaming, $filter) {
        //var limitTypes = {
        //    deposit: 'deposit',
        //    depositPerDay: 'depositPerDay',
        //    depositPerWeek: 'depositPerWeek',
        //    depositPerMonth: 'depositPerMonth',
        //    wagering: 'wagering',
        //    loss: 'loss',
        //    session: 'session'
        //}

        //$scope.getAmount = function (limit) {
        //    return limit.current
        //        ? {
        //            current: getAmountText(limit.current),
        //            queued: limit.queued ? getAmountText(limit.queued) : null
        //        }
        //        : "No limit";
        //};
        //function getAmountText(currency, amount, period) {
        //    return currency + " " + amount + " / " + getAmountPeriod(period);
        //}
        //function getAmountPeriod(period) {
        //    switch (period) {
        //        case "daily": return "day";
        //        case "weekly": return "week";
        //        case "monthly": return "month";
        //        default: return "day";
        //    }
        //}

        //$scope.getStatus = function (limit) {
        //    return limit.queued ? "Queued" : "Active";
        //};

        //$scope.getExpiration = function (limit) {
        //    return limit.current
        //        ? $filter('date')(limit.current.expiryDate)
        //        : "No expiration";
        //};


        //emResponsibleGaming.getLimits().then(function (response) {
        //    var limits = response;
        //    $scope.limits = {
        //        deposit: {},
        //        wagering: {},
        //        loss: {},
        //        session: {}
        //    }
        //});

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

        //function setMoneyLimit(period, amount, currency, func) {
        //};

        //emResponsibleGaming.setDepositLimit();
    }

})();