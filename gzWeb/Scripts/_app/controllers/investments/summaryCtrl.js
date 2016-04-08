(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', 'api', ctrlFactory]);
    function ctrlFactory($scope, api) {
        $scope.showAllVintages = function() {
            // TODO showAllVintages modal
        };

        $scope.transferCashToGames = function() {
            api.call(function () {
                return api.transferCashToGames();
            }, function (response) {
                // TODO toastr
            });
        };

        function init() {
            api.call(function () {
                return api.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                $scope.lastVintages = $scope.model.Vintages.slice(0, 3);
            });
        }
        init();
    }
})();