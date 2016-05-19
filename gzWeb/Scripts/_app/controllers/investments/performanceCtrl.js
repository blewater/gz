(function () {
    'use strict';
    var ctrlId = 'performanceCtrl';
    APP.controller(ctrlId, ['$scope', 'api', 'auth', ctrlFactory]);
    function ctrlFactory($scope, api, auth) {
        function loadPerformanceData() {
            api.call(function () {
                return api.getPerformanceData();
            }, function (response) {
                $scope.model = response.Result;
            });
        }

        function loadAuthData() {
            $scope.currency = auth.data.currency;
        }
        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, function () {
            loadAuthData();
            $scope.$apply();
        });

        function init() {
            loadPerformanceData();
            loadAuthData();
        }
        init();
    }
})();