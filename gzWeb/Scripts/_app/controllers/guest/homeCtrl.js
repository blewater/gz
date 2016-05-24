(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'message', ctrlFactory]);
    function ctrlFactory($scope, $location, message) {
        function readResetPwdKeys() {
            var urlParams = $location.search();
            var email = urlParams.email;
            var gzResetKey = urlParams.gzKey;
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

        function init() {
            readResetPwdKeys();
        }
        init();

        $scope.watchVideo = function() {
            message.open({
                nsType: 'modal',
                nsSize: '640px',
                nsTemplate: '/partials/messages/explainerVideo.html'
            });
        }
    }
})();