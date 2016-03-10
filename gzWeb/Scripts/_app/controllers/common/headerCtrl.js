(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'routingService', ctrlFactory]);
    function ctrlFactory($scope, constants, routingService) {
        $scope.routes = {
            guest: routingService.getGroup(constants.groupKeys.guest),
            games: routingService.getGroup(constants.groupKeys.games),
            investments: routingService.getGroup(constants.groupKeys.investments)
        }
        $scope.getClass = function (path) {
            return routingService.isCurrentPath(path) ? 'focus' : '';
        }
    }
})();