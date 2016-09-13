(function () {
    'use strict';

    APP.directive('gzFieldOk', ['helpers', directiveFactory]);

    function directiveFactory(helpers) {
        return {
            restrict: 'E',
            scope: {
                gzWhen: '&',
                gzLoading: '&'
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/directives/gzFieldOk.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                $scope.spinnerOptions = { radius: 5, width: 2, length: 4, color: '#fff', position: 'absolute', top: '50%', right: 0 };
            }]
        };
    }
})();