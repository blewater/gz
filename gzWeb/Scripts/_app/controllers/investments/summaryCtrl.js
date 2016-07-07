(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', 'api', 'message', '$location', 'constants', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $controller, api, message, $location, constants, $filter) {
        $controller('authCtrl', { $scope: $scope });

        var sellingValuesFetched = false;

        $scope.openVintages = function (title, vintages, withdrawMode) {
            return message.modal(title, {
                nsSize: '700px',
                nsTemplate: '/partials/messages/summaryVintages.html',
                nsCtrl: 'summaryVintagesCtrl',
                nsStatic: withdrawMode,
                nsParams: {
                    vintages: vintages,
                    withdrawMode: withdrawMode,
                    currency: $scope.currency
                    //model: $scope.model
                }
            });
        };

        $scope.showAllVintages = function () {
            $scope.openVintages('Vintages history', $scope.vintages, false);
        };

        $scope.withdraw = function () {
            withdrawVintages();
            //if (!sellingValuesFetched) {
            //    api.call(function() {
            //        return api.getVintagesWithSellingValues();
            //    }, function (getVintagesRsponse) {
            //        sellingValuesFetched = true;
            //        $scope.vintages = processVintages(getVintagesRsponse.Result);
            //        withdrawVintages();
            //    });
            //}
            //else
            //    withdrawVintages();
        };

        function withdrawVintages() {
            var promise = $scope.openVintages('Available funds for withdrawal', $scope.vintages, true);
            promise.then(function (updatedVintages) {
                api.call(function () {
                    return api.withdrawVintages(updatedVintages);
                }, function (withdrawResponse) {
                    $scope.vintages = processVintages(withdrawResponse.Result);
                    message.success("Your wallet balance has been updated. Please check your casino account over the next few days.");
                }, {
                    rejectFn: function() {
                        var selectedCount = $filter('where')(updatedVintages, { 'Selected': true }).length;
                        var failureMsg =
                            "Our attempt to sell your investment vintage" +
                            (selectedCount === 1 ? "" : "s") +
                            " has not been successful. We apologize for the inconvenience. Please try again later.";
                        message.error(failureMsg);
                    }
                });
            }, function(error) {
                for (var i = 0; i < $scope.vintages.length; i++)
                    $scope.vintages[i].Selected = false;
            });
        }

        $scope.backToGames = function() {
            $location.path(constants.routes.games.path);
        };

        function processVintages(vintages) {
            var mappedVintages = $filter('map')(vintages, function (v) {
                v.Year = parseInt(v.YearMonthStr.slice(0, 4));
                v.Month = parseInt(v.YearMonthStr.slice(-2));
                v.Date = new Date(v.Year, v.Month - 1);
                return v;
            });
            var ordered = $filter('orderBy')(mappedVintages, 'Date', true);
            return ordered;
        }

        function loadSummaryData() {
            api.call(function () {
                return api.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                $scope.model.OkToWithdraw = false;
                $scope.vintages = processVintages($scope.model.Vintages);
            });            
        }

        function loadAuthData() {
            $scope.hasGamingBalance = $scope._authData.gamingAccount !== undefined;
            if ($scope.hasGamingBalance)
                $scope.gamingBalance = $scope._authData.gamingAccount.amount;
            $scope.currency = $scope._authData.currency;
        }

        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);

        $scope._init('summary', function() {
            loadSummaryData();
            loadAuthData();
        });
    }
})();