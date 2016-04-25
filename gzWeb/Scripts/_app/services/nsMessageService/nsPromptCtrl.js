(function () {
    'use strict';
    var ctrlId = 'nsPromptCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope._dispatchEventKeys = function(event, obj) {
            if (obj.key === 'ok')
                $scope.nsOk(true);
            else if (obj.key === 'cancel')
                $scope.nsCancel("cancel");
        };
        var unregisterBtnClickEvent = $scope.$on('btnClicked', function (event, obj) {
            if (angular.isDefined($scope.dispatchEventKeys))
                $scope.dispatchEventKeys(event, obj);
            else
                $scope._dispatchEventKeys(event, obj);
        });
        $scope.$on("$destroy", function () {
            unregisterBtnClickEvent();
        });
    }
})();
