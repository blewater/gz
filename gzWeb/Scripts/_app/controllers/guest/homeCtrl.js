(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'message', ctrlFactory]);
    function ctrlFactory($scope, $location, message) {
        function readResetPwdKeys() {
            var urlParams = $location.search();
            var gzResetKey = urlParams.gzKey;
            var emResetKey = urlParams.emKey;
            console.log(emResetKey);
            if (emResetKey) {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '/partials/messages/resetPassword.html',
                    nsCtrl: 'resetPasswordCtrl',
                    nsStatic: true,
                    nsParams: {
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
    }
})();