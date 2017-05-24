(function () {
    'use strict';
    var ctrlId = 'transactionHistoryCtrl';
    APP.controller(ctrlId, ['$scope', 'emBanking', '$timeout', '$filter', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emBanking, $timeout, $filter, constants) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        // #region Transaction Types
        // #region Common Functions
        function getId(trx) {
            return trx.transactionID;
        }
        function getDate(trx) {
            return $filter('date')(moment.utc(trx.time).toDate(), 'dd/MM/yy HH:mm');
        }
        function getAmountText(amount, currency) {
            return $filter('isoCurrency')(amount, currency, 2);
        }
        function getStatus(trx) {
            return trx.status || 'Success';
        }
        // #endregion

        // #region Deposit
        var deposit = {
            name: 'Deposit',
            display: 'Deposit',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) { return trx.debit.amount; },
            getAmountText: function (trx) { return getAmountText(trx.debit.amount, trx.debit.currency); },
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
            getAmount: function (trx) { return trx.credit.amount; },
            getAmountText: function (trx) { return getAmountText(trx.credit.amount, trx.credit.currency); },
            getDescription: function (trx) { return trx.credit.name; },
            getStatus: getStatus
        };
        // #endregion

        // #region Transfer
        var transfer = {
            name: 'Transfer',
            display: 'Transfer',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) { return trx.credit.amount; },
            getAmountText: function (trx) { return getAmountText(trx.credit.amount, trx.credit.currency); },
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
            getAmount: function (trx) { return trx.credit.amount; },
            getAmountText: function (trx) { return getAmountText(trx.credit.amount, trx.credit.currency); },
            getDescription: function (trx) { return trx.debit.name; },
            getStatus: getStatus
        };
        // #endregion

        // #region GamingTransfer
        var gamblingTransfer = {
            name: 'Gambling',
            display: 'Gambling',
            getId: getId,
            getDate: getDate,
            getAmount: function (trx) {
                return trx.credit === undefined ? trx.debit.amount : trx.credit.amount;
            },
            getAmountText: function (trx) {
                var currency = trx.credit === undefined ? trx.debit.currency : trx.credit.currency;
                return (trx.credit === undefined
                            ? "-" + getAmount(trx.debit.amount, trx.debit.currency)
                            : getAmountText(trx.credit.amount, trx.credit.currency)
                    ) + " / " +
                    $filter('isoCurrency')(trx.balance, currency, 2);
            },
            getDescription: function(trx) {
                return (trx.credit === undefined
                    ? trx.debit.name
                    : trx.credit.name) + " - " + trx.description;
            },
            getStatus: getStatus
        };
        // #endregion

        $scope.transactionTypes = [deposit, withdraw, gamblingTransfer];//, transfer, buddyTransfer];
        // #endregion

        // #region Filters
        var pageSize = 10;
        $scope.type = deposit;
        $scope.pageIndex = 1;
        $scope.transactions = undefined;
        $scope.totalRecordCount = 0;
        $scope.totalPageCount = 0;

        $scope.from = {
            value: moment().subtract(1, 'months').startOf('day').toDate(),
            open: false,
            options: {
                maxDate: moment().endOf('day').toDate(),
            }
        };
        $scope.to = {
            value: moment().endOf('day').toDate(),
            open: false,
            options: {
                minDate: moment().subtract(1, 'months').startOf('day').toDate(),
                maxDate: moment().endOf('day').toDate(),
            }
        };
        var unwatchFrom = $scope.$watch("from.value", function (oldValue, newValue) {
            $scope.to.options.minDate = newValue;
        });
        var unwatchTo = $scope.$watch("to.value", function (oldValue, newValue) {
            $scope.from.options.maxDate = newValue;
        });
        $scope.$on('$destroy', function () {
            unwatchFrom();
            unwatchTo();
        });
        // #endregion

        // #region Search
        $scope.searching = false;
        $scope.search = function (page) {
            $scope.searching = true;
            emBanking.getTransactionHistory($scope.type.name, $scope.from.value.toISOString(), $scope.to.value.toISOString(), page, pageSize).then(function (response) {
                $timeout(function () {
                    $scope.searching = false;
                    $scope.pageIndex = response.currentPageIndex;
                    $scope.transactions = [];//response.transactions;
                    for (var i = 0; i < response.transactions.length; i++) {
                        if ($scope.type.getAmount(response.transactions[i]) > 0) {
                            $scope.transactions.push({
                                id: $scope.type.getId(response.transactions[i]),
                                date: $scope.type.getDate(response.transactions[i]),
                                amount: $scope.type.getAmountText(response.transactions[i]),
                                description: $scope.type.getDescription(response.transactions[i]),
                                status: $scope.type.getStatus(response.transactions[i])
                            });
                        }
                    }
                    $scope.totalRecordCount = response.totalRecordCount;
                    $scope.totalPageCount = response.totalPageCount;
                }, 0);
            });
        }
        $scope.search(1);
        // #endregion
    }
})();