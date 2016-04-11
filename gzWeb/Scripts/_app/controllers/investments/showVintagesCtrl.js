(function () {
    'use strict';
    var ctrlId = 'showVintagesCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, $filter, $timeout) {
        function init() {
            var mapped = $filter('map')($scope.vintages, function (v) {
                v.Date = new Date(v.Date);
                v.Year = v.Date.getFullYear();
                v.Month = v.Date.getMonth();
                return v;
            });
            var ordered = $filter('orderBy')(mapped, 'Date', true);
            var grouped = $filter('groupBy')(ordered, 'Year');
            var array = $filter('toArray')(grouped, true);
            var result = $filter('orderBy')(array, '$key', true);
            $scope.vintagesPerYear = result;
            //$scope.vintagesPerYear =
            //    $filter('orderBy')
            //        ($filter('toArray')
            //            ($filter('groupBy')
            //                ($filter('orderBy')
            //                    ($filter('map')
            //                        ($scope.vintages,
            //                        function (v) {
            //                            v.Year = v.Date.getFyllYear();
            //                            v.Month = v.Date.getMonth();
            //                            return v;
            //                        }),
            //                    'Date',
            //                    true),
            //                'Year'),
            //            true),
            //        '$key');
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