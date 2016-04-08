(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', 'route', ctrlFactory]);
    function ctrlFactory($scope, $location, constants, route) {
        $scope.routes = {
            guest: route.getGroup(constants.groupKeys.guest),
            games: route.getGroup(constants.groupKeys.games),
            investments: route.getGroup(constants.groupKeys.investments)
        }
        $scope.getClass = function (path) {
            return route.isCurrentPath(path) ? 'focus' : '';
        }

        $scope.backToGames = function () {
            $scope.gamesMode = !$scope.gamesMode;
            $location.path($scope.routes.games[0].path);
        };
        $scope.toInvestments = function () {
            $scope.gamesMode = !$scope.gamesMode;
            $location.path($scope.routes.investments[0].path);
        };
    }
})();