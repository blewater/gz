(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'message', ctrlFactory]);
    function ctrlFactory($scope, $location, message) {
        function readResetKey() {
            var urlParams = $location.search();
            var resetPwdKey = urlParams.resetKey;
            console.log(resetPwdKey);
            if (resetPwdKey) {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '/partials/messages/resetPassword.html',
                    nsCtrl: 'resetPasswordCtrl',
                    nsStatic: true,
                    nsParams: { resetKey: resetPwdKey }
                });
            }
        }

        function init() {
            readResetKey();
        }
        init();
    }
})();