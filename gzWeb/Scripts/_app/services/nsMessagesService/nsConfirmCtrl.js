(function () {
    'use strict';
    var ctrlId = 'nsConfirmCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $controller) {
        $controller('nsPromptCtrl', { $scope: $scope });
        $scope.dispatchEventKeys = function (event, obj) {
            if (obj.key === 'ok')
                $scope.nsOk(true);
            else if (obj.key === 'cancel')
                $scope.nsCancel('cancel');
        };
    }
})();
