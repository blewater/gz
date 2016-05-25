(function () {
    'use strict';
    var ctrlId = 'authCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'auth', ctrlFactory]);
    function ctrlFactory($scope, constants, auth) {
        function loadAuthData() {
            $scope._authData = auth.data;
        }

        $scope._init = function (ctrl, initCallback) {
            function callback() {
                loadAuthData();
                if (angular.isFunction(initCallback))
                    initCallback();
            }

            if ($scope.initialized)
                callback();
            else {
                var unregister = $scope.$on(constants.events.ON_INIT, function () {
                    callback();
                    unregister();
                });
            }
        }
    }
})();