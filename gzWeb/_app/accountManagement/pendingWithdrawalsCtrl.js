(function () {
    'use strict';
    var ctrlId = 'pendingWithdrawalsCtrl';
    APP.controller(ctrlId, ['$scope', 'emBankingWithdraw', '$filter', ctrlFactory]);
    function ctrlFactory($scope, emBankingWithdraw, $filter) {
        $scope.pendingWithdrawals = undefined;
        emBankingWithdraw.getPendingWithdrawals().then(function (response) {
            $scope.pendingWithdrawals = response || [];
        });

        $scope.getDate = function (pendingWithdrawal) { return $filter('date')(pendingWithdrawal.time, 'dd/MM/yy HH:mm'); }
        $scope.getAmount = function (pendingWithdrawal) { return $filter('isoCurrency')(pendingWithdrawal.credit.amount, pendingWithdrawal.credit.currency, 2); }
        $scope.getDescription = function (pendingWithdrawal) { return pendingWithdrawal.credit.name; }
    }
})();