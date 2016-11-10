(function () {
    'use strict';
    var ctrlId = 'depositMoneyMatrixTrustlyCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217) {
        $scope.model = {
            amount: undefined,
            bonusCode: undefined
        };

        function loadCreditCardInfo() {
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount (between " + $scope.accountLimits.min + " and " + $scope.accountLimits.max + ")";
        }

        function init() {
            loadCreditCardInfo();
        };

        function getFields() {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                bonusCode: $scope.model.bonusCode
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            q.resolve(getFields());
            return q.promise;
        }

        $scope.readConfirmMessage = function (prepareData) {
            return "Do you want to deposit the amount of " + prepareData.creditAmount + " using Trustly?";
        };

        init();
    }
})();