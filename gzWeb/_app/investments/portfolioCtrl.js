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
            window.appInsights.trackEvent("SELECT PORTFOLIO (" + plan.Title + ")");
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
                loadingFn: function (flag) { plan.selecting = flag; }
            });
        }

        function loadPortfolioData() {
            api.call(function () {
                return api.getPortfolioData();
            }, function (response) {
                $scope.model = response.Result;
                $scope.model.SelectedPlan = $filter('filter')($scope.model.Plans, { Selected: true })[0];
                $scope.model.SelectedIndex = $scope.model.Plans.indexOf($scope.model.SelectedPlan);
            });
        }

        function loadAuthData() {
            $scope.currency = $scope._authData.currency;
        }

        $scope._init(function () {
            window.appInsights.trackEvent("INVESTMENT PORTFOLIO");
            loadPortfolioData();
            loadAuthData();
            $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);
        });
    }
})();