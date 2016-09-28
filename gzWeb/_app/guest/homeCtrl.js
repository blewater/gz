(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, message, constants) {
        $controller('authCtrl', { $scope: $scope });

        function readForgotPwdEmail(urlParams) {
            var forgot = urlParams.forgot;
            var email = urlParams.email;
            if (forgot === 'true' && email) {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '_app/account/forgotPassword.html',
                    nsCtrl: 'forgotPasswordCtrl',
                    nsStatic: true,
                    nsParams: {
                        email: email
                    }
                });
            }
        }

        function readResetPwdKeys(urlParams) {
            var email = urlParams.email;
            var gzResetKey = decodeURIComponent(urlParams.gzKey);
            var emResetKey = urlParams.emKey;
            if (emResetKey) {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '_app/account/resetPassword.html',
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

        function readLogoutReason(urlParams) {
            var logoutReason = urlParams.logoutReason;
            if (logoutReason) {
                message.info(logoutReason);
                $location.search('');
            }
        }

        //$scope.watchVideo = function() {
        //    message.open({
        //        nsType: 'modal',
        //        nsSize: '640px',
        //        nsTemplate: '_app/guest/explainerVideo.html'
        //    });
        //}

        $scope.signup = function() {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/registerAccount.html',
                nsCtrl: 'registerAccountCtrl',
                nsStatic: true
            });
        };

        $scope._init(function () {
            var urlParams = $location.search();
            if (Object.keys(urlParams).length > 0) {
                readResetPwdKeys(urlParams);
                readLogoutReason(urlParams);
                readForgotPwdEmail(urlParams);
            }
            else if ($scope._authData.isGamer)
                $location.path(constants.routes.games.path).search({});
            //else
            //    readParams();
        });
    }
})();