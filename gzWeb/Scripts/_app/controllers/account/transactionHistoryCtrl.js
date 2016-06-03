(function () {
    'use strict';
    var ctrlId = 'transactionHistoryCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope.fillTable = [];
        for (var i = 0; i < 7; i++) {
            var text = "lorem ipsum";
            $scope.fillTable.push(text);
        };
    }
})();