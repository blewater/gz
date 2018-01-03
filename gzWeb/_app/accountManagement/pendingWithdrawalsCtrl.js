(function () {
    'use strict';
    var ctrlId = 'pendingWithdrawalsCtrl';
    APP.controller(ctrlId, ['$scope', 'emBankingWithdraw', '$filter', 'constants', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, emBankingWithdraw, $filter, constants, $timeout) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        function init() {
            $scope.pendingWithdrawals = undefined;
            emBankingWithdraw.getPendingWithdrawals().then(function (response) {
                var withdrawals = response || [];
                angular.forEach(withdrawals, function (withdrawal) {
                    withdrawal.date = getDate(withdrawal);
                    withdrawal.amount = getAmount(withdrawal);
                    withdrawal.description = getDescription(withdrawal);
                });
                $scope.pendingWithdrawals = withdrawals;
            });
        };
        init();

        function getDate(pendingWithdrawal) {
            return $filter('date')(pendingWithdrawal.time, 'dd/MM/yy HH:mm');
        }
        function getAmount(pendingWithdrawal) {
            return pendingWithdrawal.credit
                ? $filter('isoCurrency')(pendingWithdrawal.credit.amount, pendingWithdrawal.credit.currency, 2)
                : $filter('isoCurrency')(pendingWithdrawal.debit.amount, pendingWithdrawal.debit.currency, 2);
        }
        function getDescription(pendingWithdrawal) {
            return pendingWithdrawal.credit ? pendingWithdrawal.credit.name : pendingWithdrawal.debit.name;
        }

        $scope.rollback = function (pendingWithdrawal, index) {
            pendingWithdrawal.cancelling = true;
            emBankingWithdraw.rollback(pendingWithdrawal.id).then();
            $timeout(function () {
                pendingWithdrawal.cancelling = false;
                $scope.pendingWithdrawals.splice(index, 1);
            }, 1000);
        }
    }
})();