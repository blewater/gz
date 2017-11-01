(function () {
    'use strict';
    var ctrlId = 'noGameFoundCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.backToGames = function () {
            $scope.nsOk(true);
            //$location.path(constants.routes.games.path).search($scope.searchParams);
        }
    }
})();
