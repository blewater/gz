(function () {
    'use strict';
    var ctrlId = 'privacyGamesCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.gotoInvestmentPrivacy = function () {
            $location.path(constants.routes.privacyInvestment.path);
        };
    }
})();