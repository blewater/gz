(function () {
    'use strict';
    var ctrlId = 'activityCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $controller) {
        $controller('authCtrl', { $scope: $scope });
    }
})();