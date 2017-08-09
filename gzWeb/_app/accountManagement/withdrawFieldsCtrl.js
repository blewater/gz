(function () {
    'use strict';
    var ctrlId = 'withdrawFieldsCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, $filter) {
        $scope.model = {
            amount: undefined,
        };

        function loadCreditCardInfo() {
            $scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.accountLimitMax = Math.min($scope.accountLimits.max, $scope.gamingAccount.amount);
            $scope.limitMin = $scope.accountLimits.min;
            $scope.limitMax = $scope.accountLimitMax;
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

        init();
    }
})();