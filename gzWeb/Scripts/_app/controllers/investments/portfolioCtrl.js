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
                return api.setPlanSelection(plan);
            }, function (response) {
                var index = $scope.model.Plans.indexOf(plan);
                for (var i = 0; i < $scope.model.Plans.length; i++)
                    $scope.model.Plans[i].Selected = index === i;
                $scope.model.SelectedIndex = index;
                $scope.model.SelectedPlan = $scope.model.Plans[index];

                message.success('Your selection was registered successfully!', { nsType: 'toastr' });
            }, {
                loadingFn: function (flag) { plan.selecting = flag; },
                errorFn: function(error) { message.success(error); }
            });
        }

        function loadPortfolioData() {
            api.call(function () {
                return api.getPortfolioData();
            }, function (response) {
                $scope.model = response.Result;
                for (var i = 0; i < $scope.model.Plans.length; i++) {
                    //if (!$scope.model.Plans[i].AllocatedPercent) {
                    //    $scope.model.Plans[i].AllocatedPercent = Math.round((i * (100 / $scope.model.Plans.length)) * 10) / 10;
                    //    $scope.model.Plans[i].AllocatedAmount = 256000 * ($scope.model.Plans[i].AllocatedPercent / 100);
                    //}
                }
                $scope.model.SelectedPlan = $filter('filter')($scope.model.Plans, { Selected: true })[0];
                $scope.model.SelectedIndex = $scope.model.Plans.indexOf($scope.model.SelectedPlan);
            });
        }

        function loadAuthData() {
            $scope.currency = $scope._authData.currency;
        }

        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);

        $scope._init('portfolio', function () {
            loadPortfolioData();
            loadAuthData();
        });
    }
})();