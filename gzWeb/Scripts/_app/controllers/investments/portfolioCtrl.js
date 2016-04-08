(function () {
    'use strict';
    var ctrlId = 'portfolioCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'api', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $filter, api, constants) {
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
            }, {
                loadingFn: function (flag) { plan.selecting = flag; }
            });
        }

        function init() {
            api.call(function () {
                return api.getPortfolioData();
            }, function (response) {
                $scope.model = response.Result;
            });
        }
        init();
    }
})();