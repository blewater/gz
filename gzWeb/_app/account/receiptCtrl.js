(function () {
    'use strict';
    var ctrlId = 'receiptCtrl';
    APP.controller(ctrlId, ['$scope', 'iso4217', '$timeout', '$rootScope', 'constants', ctrlFactory]);
    function ctrlFactory($scope, iso4217, $timeout, $rootScope, constants) {
        $scope.spinnerOptions = { radius: 5, width: 2, length: 4, color: '#fff', position: 'absolute', top: '50%' };
        var MAX_ATTEMPTS_NO = 5;
        var DELAY = 3000;
        var currentAttemptNo = 0;

        function chooseTitle(status) {
            switch (status) {
                case 'success':
                    return 'Your transaction has completed successfully!';
                case 'incomplete':
                    return 'Your transaction is incomplete!';
                case 'error':
                    return 'Your transaction failed!';
                case 'pending':
                    return 'Your transaction is pending and waiting for approval!';
                default:
                    return 'Unknown transaction status!!!';
            }
        }

        $scope.onOk = function () {
            $scope.nsOk($scope.transactionInfo);
        };
        function getTransactionInfo() {
            currentAttemptNo++;
            $scope.getTransactionInfoCall().then(function (transactionResult) {
                assignToScope(transactionResult);
            }, function (error) {
                $scope.nsCancel(error);
            });
        }

        function assignToScope(transactionInfo) {
            $scope.transactionInfo = transactionInfo;
            $scope.title = chooseTitle($scope.transactionInfo.status);
            $scope.transactionId = $scope.transactionInfo.transactionID;
            $scope.status = $scope.transactionInfo.status;
            $scope.time = moment.utc($scope.transactionInfo.time).toDate().toLocaleString();
            $scope.desc = $scope.transactionInfo.desc;
            var amountInfo = $scope.isDebit ? $scope.transactionInfo.debit : $scope.transactionInfo.credit;
            if (amountInfo)
                $scope.amount = iso4217.getCurrencyByCode(amountInfo.currency).symbol + " " + amountInfo.amount;
            $scope.paymentMethod = $scope.paymentMethod;

            $scope.stillWaiting = currentAttemptNo <= MAX_ATTEMPTS_NO;
            if ($scope.isDebit && !$scope.transactionId && $scope.status === 'pending' && $scope.stillWaiting)
                $timeout(getTransactionInfo, DELAY);
        }

        function init() {
            getTransactionInfo();
        }

        init();
    }
})();