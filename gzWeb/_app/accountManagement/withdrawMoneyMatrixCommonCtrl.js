(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixCommonCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$q', 'iso4217', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $q, iso4217, $filter) {
        $scope.model = {
            amount: undefined
        };

        $scope._init = function() {
            if (angular.isFunction($scope.getExtraModelProperties))
                angular.extend($scope.model, $scope.getExtraModelProperties());

            loadPayCardInfo();
        };

        function loadPayCardInfo() {
            loadPayCardGenericInfo();
            if (angular.isDefined($scope.loadPayCardSpecificInfo) && angular.isFunction($scope.loadPayCardSpecificInfo))
                $scope.loadPayCardSpecificInfo();
        }
        function loadPayCardGenericInfo() {
            //$scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
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
        }

        $scope.readFields = function () {
            var q = $q.defer();
            var fields = {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
            }
            if (angular.isFunction($scope.getExtraWithdrawFields))
                angular.extend(fields, $scope.getExtraWithdrawFields());

            if ($scope.selected.method)
                fields.payCardID = $scope.selected.method.id;
            else if (angular.isFunction($scope.getRegistrationFields))
                angular.extend(fields, $scope.getRegistrationFields());

            q.resolve(fields);
            return q.promise;
        };

        $scope.readConfirmMessage = function (prepareData) {
            var confirmMessage = angular.isFunction($scope.getSpecificConfirmMessage)
                ? $scope.getSpecificConfirmMessage(prepareData)
                : "Do you want to withdraw the amount of " + prepareData.debitAmount + " using " + $scope.selected.group[0].name + "?";
            return confirmMessage;
        };
    }
})();