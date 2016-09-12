(function () {
    'use strict';
    var ctrlId = 'confirmWithdrawCtrl';
    APP.controller(ctrlId, ['$scope', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, iso4217) {
        $scope.creditTo = $scope.prepareResult.credit.name;
        $scope.creditAmount = iso4217.getCurrencyByCode($scope.prepareResult.credit.currency).symbol + " " + $scope.prepareResult.credit.amount;
        $scope.debitFrom = $scope.prepareResult.debit.name;
        $scope.debitAmount = iso4217.getCurrencyByCode($scope.prepareResult.debit.currency).symbol + " " + $scope.prepareResult.debit.amount;
    }
})();