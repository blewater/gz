(function () {
    'use strict';
    var ctrlId = 'authCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'auth', ctrlFactory]);
    function ctrlFactory($scope, constants, auth) {
        function loadAuthData() {
            $scope._authData = auth.data;
        }

        $scope._init = function (initCallback) {
            function callback() {
                loadAuthData();
                if (angular.isFunction(initCallback))
                    initCallback();
            }

            if ($scope.initialized)
                callback();
            else {
                var unregisterAfterInit = $scope.$on(constants.events.ON_AFTER_INIT, function () {
                    if ($scope.redirected) {
                        var unregisterRedirected = $scope.$on(constants.events.REDIRECTED, function () {
                            callback();
                            unregisterRedirected();
                        });
                    }
                    else
                        callback();
                    unregisterAfterInit();
                });
            }
        }
    }
})();