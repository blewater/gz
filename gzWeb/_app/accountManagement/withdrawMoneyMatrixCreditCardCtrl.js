(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', 'auth', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, auth, $filter) {
        $scope.model = {
            amount: undefined,
        };

        function loadCreditCardInfo() {
            $scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.accountLimitMax = Math.min($scope.accountLimits.max, auth.data.gamingAccounts[0].amount);
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount (between " + $filter('number')($scope.accountLimits.min, 2) + " and " + $filter('number')($scope.accountLimitMax, 2) + ")";
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
            return "Do you want to withdraw the amount of " + prepareData.debitAmount + " to " + prepareData.creditTo + "?";
        };

        init();
    }
})();