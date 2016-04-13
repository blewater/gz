(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', 'api', 'message', ctrlFactory]);
    function ctrlFactory($scope, api, message) {
        $scope.showAllVintages = function () {
            message.modal('Vintages history', {
                nsSize: 'md',
                nsTemplate: '/partials/messages/showVintages.html',
                nsCtrl: 'showVintagesCtrl',
                nsParams: { vintages: $scope.model.Vintages }
            });
        };

        $scope.transferCashToGames = function() {
            api.call(function () {
                return api.transferCashToGames(); 
            }, function (response) {
                message.toastr('Your cash was transfered successfully!');
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