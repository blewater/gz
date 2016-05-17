(function () {
    'use strict';
    var ctrlId = 'summaryVintagesCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, $filter, $timeout) {
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
    }
})();