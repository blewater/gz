(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', 'routingService', ctrlFactory]);
    function ctrlFactory($scope, $location, constants, routingService) {
        $scope.routes = {
            guest: routingService.getGroup(constants.groupKeys.guest),
            games: routingService.getGroup(constants.groupKeys.games),
            investments: routingService.getGroup(constants.groupKeys.investments)
        }
        $scope.getClass = function (path) {
            return routingService.isCurrentPath(path) ? 'focus' : '';
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