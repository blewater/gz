(function () {
    'use strict';

    APP.directive('gzPerformanceGraph', [directiveFactory]);

    function directiveFactory() {
        return {
            restrict: 'A',
            scope: {
            },
            link: function (scope, element, attrs) {
            }
        };
    }
})();