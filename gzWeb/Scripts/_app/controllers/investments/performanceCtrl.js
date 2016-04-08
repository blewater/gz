(function () {
    'use strict';
    var ctrlId = 'performanceCtrl';
    APP.controller(ctrlId, ['$scope', 'api', ctrlFactory]);
    function ctrlFactory($scope, api) {
        function init() {
            api.call(function () {
                return api.getPerformanceData();
            }, function (response) {
                $scope.model = response.Result;
            });
        }
        init();
    }
})();