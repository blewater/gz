﻿(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, message, constants) {
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
                $location.search('');
            }
        }

        //$scope.watchVideo = function() {
        //    message.open({
        //        nsType: 'modal',
        //        nsSize: '640px',
        //        nsTemplate: '/partials/messages/explainerVideo.html'
        //    });
        //}

        $scope.signup = function() {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerAccount.html',
                nsCtrl: 'registerAccountCtrl',
                nsStatic: true
            });
        };

        $scope._init(function () {
            if ($scope._authData.isGamer){
                $location.path(constants.routes.games.path);
            }
            else {
                readResetPwdKeys();
                readLogoutReason();
            }
        });
    }
})();