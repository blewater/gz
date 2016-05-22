(function () {
    'use strict';
    var ctrlId = 'performanceCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', 'api', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $controller, api, constants) {
        $controller('authCtrl', { $scope: $scope });

        function loadPerformanceData() {
            api.call(function () {
                return api.getPerformanceData();
            }, function (response) {
                $scope.model = response.Result;
            });
        }

        function loadAuthData() {
            $scope.currency = $scope._authData.currency;
        }

        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, function () {
            loadAuthData();
            $scope.$apply();
        });

        $scope._init('performance', function () {
            loadPerformanceData();
            loadAuthData();
        });
    }
})();