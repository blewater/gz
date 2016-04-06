(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', 'apiService', ctrlFactory]);
    function ctrlFactory($scope, apiService) {
        $scope.showAllVintages = function() {
            // TODO showAllVintages modal
        };

        $scope.transferCashToGames = function() {
            apiService.call(function () {
                return apiService.transferCashToGames();
            }, function (response) {
                // TODO toastr
            });
        };

        function init() {
            apiService.call(function () {
                return apiService.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                $scope.lastVintages = $scope.model.Vintages.slice(0, 3);
            });
        }
        init();
    }
})();