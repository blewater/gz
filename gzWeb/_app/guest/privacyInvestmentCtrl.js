(function () {
    'use strict';
    var ctrlId = 'privacyInvestmentCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.gotoGamingPrivacy = function () {
            $location.path(constants.routes.privacyGames.path);
        };
    }
})();