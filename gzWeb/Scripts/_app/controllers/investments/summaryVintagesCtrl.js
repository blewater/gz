(function () {
    'use strict';
    var ctrlId = 'summaryVintagesCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', '$timeout', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, $filter, $timeout, helpers) {
        function init() {
            var grouped = $filter('groupBy')($scope.vintages, 'Year');
            var array = $filter('toArray')(grouped, true);
            var result = $filter('orderBy')(array, '$key', true);
            $scope.vintagesPerYear = result;
        };
        init();

        $scope.toggleExpandCollapse = function (group) {
            if (group.isCollapsed) {
                group.hide = !group.hide;
                $timeout(function() {
                    group.isCollapsed = !group.isCollapsed;
                }, 10);
            } else {
                group.isCollapsed = !group.isCollapsed;
                $timeout(function () {
                    group.hide = !group.hide;
                }, 400);
            }
        }

        function getFlattened() {
            return $filter('flatten')($scope.vintagesPerYear);
        }

        $scope.thereIsNoSelectedVintage = function() {
            return helpers.array.all(getFlattened(), function (v) {
                return !v.SellThisMonth || v.Sold;
            });
        };

        $scope.totalGain = function () {
            var flattened = getFlattened();
            var selected = $filter('where')(flattened, {'SellThisMonth': true, 'Sold': false});
            return helpers.array.aggregate(selected, 0, function (s, v) {
                return s + (v.SellingValue - v.InvestAmount);
            });
        };

        $scope.withdraw = function() {
            $scope.nsOk(getFlattened());
        };
    }
})();