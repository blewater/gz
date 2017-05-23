(function () {
    'use strict';
    var ctrlId = 'apiErrorCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.gotoGames = function () {
            window.appInsights.trackEvent("GOTO GAMES", { from: "ERROR MODAL" });
            $location.path(constants.routes.games.path).search({});
            $scope.nsOk(true);
        };
    }
})();