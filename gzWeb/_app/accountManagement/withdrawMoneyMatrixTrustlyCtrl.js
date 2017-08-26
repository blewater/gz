(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixTrustlyCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', '$filter', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, $filter, $controller) {
        $controller('withdrawMoneyMatrixCommonCtrl', { $scope: $scope });

        $scope._init();
    }
})();