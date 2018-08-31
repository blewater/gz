(function () {
    'use strict';
    var ctrlId = 'offlineCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope.backToSite = function() {
            $scope.nsOk(true);
        };
    }
})();