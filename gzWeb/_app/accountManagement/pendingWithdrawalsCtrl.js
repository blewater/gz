(function () {
    'use strict';
    var ctrlId = 'pendingWithdrawalsCtrl';
    APP.controller(ctrlId, ['$scope', 'emBankingWithdraw', '$filter', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emBankingWithdraw, $filter, constants) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        function init() {
            $scope.pendingWithdrawals = undefined;
            emBankingWithdraw.getPendingWithdrawals().then(function (response) {
                $scope.pendingWithdrawals = response || [];
            });
        };
        init();

        $scope.getDate = function (pendingWithdrawal) {
            return $filter('date')(pendingWithdrawal.time, 'dd/MM/yy HH:mm');
        }
        $scope.getAmount = function (pendingWithdrawal) {
            return $filter('isoCurrency')(pendingWithdrawal.credit.amount, pendingWithdrawal.credit.currency, 2);
        }
        $scope.getDescription = function (pendingWithdrawal) {
            return pendingWithdrawal.credit.name;
        }

        $scope.rollback = function (pendingWithdrawal, index) {
            pendingWithdrawal.cancelling = true;
            emBankingWithdraw.rollback(pendingWithdrawal.id).then(function (response) {
                pendingWithdrawal.cancelling = false;
                $scope.pendingWithdrawals.splice(index, 1);
            });
        }
    }
})();