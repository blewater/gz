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

        //$scope.getPrincipalAmount = function() {
        //    return $scope._authData.isInvestor ? $scope.model.InvestmentsBalance + $scope.model.NextExpectedInvestment : 0;
        //}
        $scope.getMonthlyContribution= function () {
            return $scope._authData.isInvestor && $scope.model.NextExpectedInvestment ? $scope.model.NextExpectedInvestment : 100;
        }

        $scope._init(function () {
            loadPerformanceData();
            loadAuthData();
            $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);
        });
    }
})();