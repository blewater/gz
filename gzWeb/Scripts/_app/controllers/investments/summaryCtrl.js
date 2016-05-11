(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', 'api', 'message', '$location', 'constants', '$filter', ctrlFactory]);
    function ctrlFactory($scope, api, message, $location, constants, $filter) {
        $scope.showAllVintages = function () {
            message.modal('Vintages history', {
                nsSize: '600px',
                nsTemplate: '/partials/messages/showVintages.html',
                nsCtrl: 'showVintagesCtrl',
                nsParams: { vintages: $scope.vintages }
            });
        };

        $scope.withdraw = function() {
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
                $scope.vintages = $filter('map')($scope.model.Vintages, function (v) {
                    v.Year = parseInt(v.YearMonthStr.slice(0, 4));
                    v.Month = parseInt(v.YearMonthStr.slice(-2));
                    v.Date = new Date(v.Year, v.Month);
                    // v.Date = new Date(v.Date);
                    // v.Year = v.Date.getFullYear();
                    // v.Month = v.Date.getMonth();
                    return v;
                });
                $scope.lastVintages = $scope.vintages.slice(0, 3);
            });            
        }

        function init() {
            getSummaryData();
        }
        init();
    }
})();