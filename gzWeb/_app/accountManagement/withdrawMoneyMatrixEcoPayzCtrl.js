(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixEcoPayzCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217) {
        $scope.model = {
            amount: undefined,
        };

        function loadCreditCardInfo() {
            $scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.limitMin = 1;//$scope.accountLimits.min;
            $scope.limitMax = $scope.accountLimits.max;
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount (between " + $filter('number')($scope.limitMin, 2) + " and " + $filter('number')($scope.limitMax, 2) + ")";
        }

        function init() {
            loadCreditCardInfo();
        };

        function getFields() {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                payCardID: $scope.payCardID.id,
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            q.resolve(getFields());
            return q.promise;
        }

        $scope.readConfirmMessage = function (prepareData) {
            return "Do you want to withdraw the amount of " + prepareData.debitAmount + " using Trustly?";
        };

        init();
    }
})();