(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', 'message', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, message) {
        $controller('authCtrl', { $scope: $scope });

        function readResetPwdKeys() {
            var urlParams = $location.search();
            var email = urlParams.email;
            var gzResetKey = decodeURIComponent(urlParams.gzKey);
            var emResetKey = urlParams.emKey;
            if (emResetKey) {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '/partials/messages/resetPassword.html',
                    nsCtrl: 'resetPasswordCtrl',
                    nsStatic: true,
                    nsParams: {
                        email: email,
                        gzKey: gzResetKey,
                        emKey: emResetKey
                    }
                });
            }
        }

        function readLogoutReason() {
            var urlParams = $location.search();
            var logoutReason = urlParams.logoutReason;
            if (logoutReason) {
                message.info(logoutReason);
            }
        }

        //$scope.watchVideo = function() {
        //    message.open({
        //        nsType: 'modal',
        //        nsSize: '640px',
        //        nsTemplate: '/partials/messages/explainerVideo.html'
        //    });
        //}

        $scope._init('summary', function() {
            readResetPwdKeys();
            readLogoutReason();
        });
    }
})();