(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', 'api', 'message', '$location', 'constants', '$filter', '$sce', ctrlFactory]);
    function ctrlFactory($scope, $controller, api, message, $location, constants, $filter, $sce) {
        $controller('authCtrl', { $scope: $scope });

        var sellingValuesFetched = false;

        $scope.openVintages = function (title, vintages, withdrawMode) {
            return message.modal(title, {
                nsSize: '700px',
                nsTemplate: '_app/investments/summaryVintages.html',
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
            if (!sellingValuesFetched) {
                api.call(function() {
                    return api.getVintagesWithSellingValues($scope.vintages);
                }, function (getVintagesRsponse) {
                    sellingValuesFetched = true;
                    $scope.vintages = processVintages(getVintagesRsponse.Result);
                    withdrawVintages();
                });
            }
            else
                withdrawVintages();
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
            $location.path(constants.routes.games.path).search({});
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

        $scope.toggleInvestmentHistory = function () {
            $scope.investmentHistoryExpanded = !$scope.investmentHistoryExpanded;
        };
        $scope.toggleGamingActivities = function () {
            $scope.gamingActivitiesExpanded = !$scope.gamingActivitiesExpanded;
        };

        function loadSummaryData() {
            api.call(function () {
                return api.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                var statusAsOfLocal = moment.utc($scope.model.StatusAsOf).toDate();
                $scope.model.StatusAsOfLocalDate = $filter('date')(statusAsOfLocal, 'MMMM d');
                $scope.model.StatusAsOfLocalTime = $filter('date')(statusAsOfLocal, 'h:mm a');
                var nextInvestmentOnLocal = moment.utc($scope.model.NextInvestmentOn).toDate();
                $scope.model.NextInvestmentOnLocalDate = $filter('date')(nextInvestmentOnLocal, 'MMMM d');
                $scope.model.NextInvestmentOnLocalTime = $filter('date')(nextInvestmentOnLocal, 'h:mm a');
                $scope.model.CurrentMonth = $filter('date')(moment.utc().toDate(), 'MMMM');
                var utcNow = moment.utc().toDate();
                var startOfMonth = new Date(utcNow.getFullYear(), utcNow.getMonth(), 1);
                var todayDate = utcNow.getDate();
                $scope.model.GamingActivitiesRange =
                    $filter('ordinalDate')(startOfMonth, 'MMMM d') +
                    (todayDate === 1 ? '' : (' - ' + $filter('ordinalDate')(utcNow, 'd')));
                $scope.model.OkToWithdraw = false;
                $scope.vintages = processVintages($scope.model.Vintages);
            });            
        }

        function loadAuthData() {
            $scope.currency = $scope._authData.currency;
        }

        $scope._init(function() {
            loadSummaryData();
            loadAuthData();
        });
    }
})();