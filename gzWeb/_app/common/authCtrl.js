(function () {
    'use strict';
    var ctrlId = 'authCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', 'constants', 'auth', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, constants, auth) {
        function loadAuthData() {
            $scope._authData = auth.data;
        }

        $scope._init = function (initCallback) {
            function callback() {
                loadAuthData();
                if (angular.isFunction(initCallback))
                    initCallback();
            }

            if ($rootScope.initialized)
                callback();
            else {
                var unregisterAfterInit = $scope.$on(constants.events.ON_AFTER_INIT, function () {
                    unregisterAfterInit();
                    if ($rootScope.redirected) {
                        var unregisterRedirected = $scope.$on(constants.events.REDIRECTED, function () {
                            unregisterRedirected();
                            callback();
                        });
                    }
                    else
                        callback();
                });
            }
        }
    }
})();