(function () {
    'use strict';
    var ctrlId = 'portfolioCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$filter', 'api', 'constants', 'message', ctrlFactory]);
    function ctrlFactory($scope, $controller, $filter, api, constants, message) {
        $controller('authCtrl', { $scope: $scope });

        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.thereIsExpanded = function () {
            return $scope.model && $filter('some')($scope.model.Plans, function (p) { return p.expanded; });
        };
        $scope.selectPlan = function (plan) {
            api.call(function () {
                return api.setPlanSelection();
            }, function (response) {
                var index = $scope.model.Plans.indexOf(plan);
                for (var i = 0; i < $scope.model.Plans.length; i++)
                    $scope.model.Plans[i].Selected = index === i;

                message.toastr('Your selection was registered successfully!');
            }, {
                loadingFn: function (flag) { plan.selecting = flag; }
            });
        }

        function loadPortfolioData() {
            api.call(function () {
                return api.getPortfolioData();
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

        $scope._init('portfolio', function () {
            loadPortfolioData();
            loadAuthData();
        });
    }
})();