(function () {
    'use strict';
    var ctrlId = 'performanceCtrl';
    APP.controller(ctrlId, ['$scope', 'apiService', ctrlFactory]);
    function ctrlFactory($scope, apiService) {
        function init() {
            apiService.call(function () {
                return apiService.getPerformanceData();
            }, function (response) {
                $scope.model = response.Result;
            });
        }
        init();
    }
})();