(function () {
    'use strict';
    var ctrlId = 'receiptCtrl';
    APP.controller(ctrlId, ['$scope', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, iso4217) {
        function chooseTitle(status) {
            switch (status) {
                case 'success':
                    return 'Your transaction has completed successfully!';
                case 'incomplete':
                    return 'Your transaction is incomplete!';
                case 'error':
                    return 'Your transaction failed!';
                case 'success':
                    return 'Your transaction has been successful!';
                case 'pending':
                    return 'Your transaction is pending and waiting for approval!';
                default:
                    return 'Unknown transaction status!!!';
            }
        }

        function init() {
            $scope.title = chooseTitle($scope.transactionInfo.status);
            $scope.transactionId = $scope.transactionInfo.transactionID;
            $scope.status = $scope.transactionInfo.status;
            $scope.time = moment.utc($scope.transactionInfo.time).toDate().toLocaleString();
            $scope.desc = $scope.transactionInfo.desc;
            var amountInfo = $scope.isDebit ? $scope.transactionInfo.debit : $scope.transactionInfo.credit;
            $scope.amount = iso4217.getCurrencyByCode(amountInfo.currency).symbol + " " + amountInfo.amount;
            $scope.paymentMethod = $scope.paymentMethod;//amountInfo.name;
        }

        init();
    }
})();