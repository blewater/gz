(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', 'api', 'message', '$location', 'constants', '$filter', ctrlFactory]);
    function ctrlFactory($scope, api, message, $location, constants, $filter) {
        $scope.showAllVintages = function () {
            message.modal('Vintages history', {
                nsSize: '600px',
                nsTemplate: '/partials/messages/summaryVintages.html',
                nsCtrl: 'summaryVintagesCtrl',
                nsParams: {
                    vintages: $scope.vintages,
                    currency: $scope.model.Currency
                }
            });
        };

        $scope.withdraw = function() {
            //message.modal('Available Portfolios for withdrawal', {
            //    nsSize: '600px',
            //    nsTemplate: '/partials/messages/summaryWithdraw.html',
            //    nsCtrl: 'summaryWithdrawCtrl',
            //    nsParams: {
            //        vintages: $scope.vintages,
            //        currency: $scope.model.Currency
            //    }
            //});
            api.call(function () {
                return api.transferCashToGames(); 
            }, function (response) {
                message.toastr('Your cash was transfered successfully!');
            });
        };

        $scope.backToGames = function() {
            $location.path(constants.routes.games.path);
        };

        function getSummaryData() {
            api.call(function () {
                return api.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                var mappedVintages = $filter('map')($scope.model.Vintages, function (v) {
                    v.Year = parseInt(v.YearMonthStr.slice(0, 4));
                    v.Month = parseInt(v.YearMonthStr.slice(-2));
                    v.Date = new Date(v.Year, v.Month - 1);
                    return v;
                });
                $scope.vintages = $filter('orderBy')(mappedVintages, 'Date', true);
                $scope.lastVintages = $scope.vintages.slice(0, 3);
            });            
        }

        function init() {
            getSummaryData();
        }
        init();
    }
})();