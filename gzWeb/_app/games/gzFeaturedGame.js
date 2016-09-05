(function () {
    'use strict';

    APP.directive('gzFeaturedGame', ['helpers', '$location', 'constants', directiveFactory]);

    function directiveFactory(helpers, $location, constants) {
        return {
            restrict: 'E',
            scope: {
                gzGame: '='
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/games/gzFeaturedGame.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                $scope.playGame = function () {
                    $location.path(constants.routes.game.path.replace(":slug", $scope.gzGame.slug));
                };
            }]
        };
    }
})();