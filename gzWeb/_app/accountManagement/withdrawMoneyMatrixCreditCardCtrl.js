﻿(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', 'auth', '$filter', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, auth, $filter, $controller) {
        $controller('withdrawMoneyMatrixCommonCtrl', { $scope: $scope });

        $scope.getSpecificConfirmMessage = function (prepareData) {
            return "Do you want to withdraw the amount of " + prepareData.debitAmount + " to " + prepareData.creditTo + "?";
        };

        if ($scope.selected.group.length === 1)
            $scope.selected.method = $scope.selected.group[0];

        $scope._init();
    }
})();