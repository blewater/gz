(function () {
    'use strict';

    APP.directive('gzFeaturedGames', ['helpers', directiveFactory]);

    function directiveFactory(helpers) {
        return {
            restrict: 'E',
            scope: {
                gzGames: '='
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('partials/directives/gzFeaturedGames.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
            }]
        };
    }
})();