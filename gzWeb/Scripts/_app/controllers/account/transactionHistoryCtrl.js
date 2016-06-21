(function () {
    'use strict';
    var ctrlId = 'transactionHistoryCtrl';
    APP.controller(ctrlId, ['$scope', 'emBanking', '$timeout', '$filter', ctrlFactory]);
    function ctrlFactory($scope, emBanking, $timeout, $filter) {
        // #region Transaction Types

        // #region Common Functions
        function getId(trx) { return trx.transactionID; }
        function getDate(trx) { return $filter('date')(trx.time, 'dd/MM/yy HH:mm'); }
        function getAmount(amount, currency) { return $filter('isoCurrency')(amount, currency, 2); }
        function getStatus(trx) { return trx.status || 'Success'; }
        // #endregion

        // #region Deposit
        var deposit = {
            name: 'Deposit',
            display: 'Deposit',
            getId: getId,
            getDate: getDate,
            getAmount: function(trx) { return getAmount(trx.credit.amount, trx.credit.currency); },
            getDescription: function (trx) { return trx.debit.name; },
            getStatus: getStatus
        };
        // #endregion

        // #region Withdraw
        var withdraw = {
            name: 'Withdraw',
            display: 'Withdraw',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) { return getAmount(trx.credit.amount, trx.credit.currency); },
            getDescription: function (trx) { return trx.debit.name; },
            getStatus: getStatus
        };
        // #endregion

        // #region Transfer
        var transfer = {
            name: 'Transfer',
            display: 'Transfer',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) { return getAmount(trx.credit.amount, trx.credit.currency); },
            getDescription: function (trx) { return trx.debit.name; },
            getStatus: getStatus
        };
        // #endregion

        // #region BuddyTransfer
        var buddyTransfer = {
            name: 'BuddyTransfer',
            display: 'Buddy Transfer',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) { return getAmount(trx.credit.amount, trx.credit.currency); },
            getDescription: function(trx) { return trx.debit.name; },
            getStatus: getStatus
        };
        // #endregion

        $scope.transactionTypes = [deposit, withdraw, transfer, buddyTransfer];
        // #endregion

        var pageSize = 10;
        $scope.type = deposit;
        $scope.startTime = moment().subtract(1, 'months').toDate();
        $scope.endTime = moment().toDate();
        $scope.pageIndex = 1;
        $scope.transactions = [];
        $scope.totalRecordCount = 0;
        $scope.totalPageCount= 0;

        $scope.startTimeOptions = {
            maxDate: $scope.endTime,
        }

        $scope.endTimeOptions = {
            minDate: $scope.startTime,
        }

        $scope.search = function (page) {
            emBanking.getTransactionHistory($scope.type.name, $scope.startTime.toISOString(), $scope.endTime.toISOString(), page, pageSize).then(function (response) {
                $timeout(function () {
                    $scope.pageIndex = response.currentPageIndex;
                    $scope.transactions = response.transactions;
                    $scope.totalRecordCount = response.totalRecordCount;
                    $scope.totalPageCount = response.totalPageCount;
                }, 0);
            });
        }
        $scope.search(1);
    }
})();