(function () {
    'use strict';
    var ctrlId = 'termsGamesCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $location, constants) {
        $scope.gotoInvestmentTerms = function () {
            $location.path(constants.routes.termsInvestment.path);
        };
    }
})();