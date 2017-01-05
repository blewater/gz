(function () {
    'use strict';
    var ctrlId = 'termsInvestmentCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.gotoGamingTerms = function () {
            $location.path(constants.routes.termsGames.path);
        };
    }
})();