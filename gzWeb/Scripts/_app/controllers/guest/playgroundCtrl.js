(function () {
    'use strict';
    var ctrlId = 'playgroundCtrl';
    APP.controller(ctrlId, ['$scope', 'message', 'emWamp', 'chat', '$location', 'auth', ctrlFactory]);
    function ctrlFactory($scope, message, emWamp, chat, $location, auth) {
        // #region Playground
        // #region Messages
        var m = 1, n = 1, t = 1;
        $scope.alert = function () {
            message.alert('Alert ' + m++);
        };
        $scope.confirm = function () {
            message.confirm('Confirm ' + m++);
        };
        $scope.prompt = function () {
            message.prompt('Prompt' + m++);
        };
        $scope.notify = function () {
            message.notify('Notification ' + n++);
        };
        $scope.toastr = function () {
            message.toastr('Toastr ' + t++);
        };

        $scope.error = function () {
            message.error('Error sdfg dtg d ghd gh dt gh  dth d ty hdbt yh  dtyhjtdyhbetdhrstgvetrheyhrfyjrct yhrvtvyjry j rvy jhjjjgujhb jbjbklhbgfnuuyh yfbnuymmntf bnmn tbyuihn tygbnyuhtygb6t7gbbntgnjj vyd j ry j vr tj y hjv ryvrh xch gh');
        };
        $scope.success = function () {
            message.success('Success');
        };
        $scope.warning = function () {
            message.warning('Warning');
        };
        $scope.info = function () {
            message.info('Info');
        };
        // #endregion

        // #region Account
        $scope.registerAccount = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerAccount.html',
                nsCtrl: 'registerAccountCtrl',
                nsStatic: true,
            });
        };
        $scope.registerDetails = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerDetails.html',
                nsCtrl: 'registerDetailsCtrl',
                nsStatic: true,
                nsParams: {
                    startModel: {
                        email: 'email',
                        username: 'username',
                        password: 'password'
                    }
                }
            });
        };
        $scope.selectPayment = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerPaymentMethods.html',
                nsCtrl: 'registerPaymentMethodsCtrl',
                nsStatic: true
            });
        };
        $scope.login = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        $scope.forgot = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/forgotPassword.html',
                nsCtrl: 'forgotPasswordCtrl',
                nsStatic: true,
            });
        };
        $scope.reset = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/resetPassword.html',
                nsCtrl: 'resetPasswordCtrl',
                nsStatic: true,
            });
        };
        $scope.change = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/changePassword.html',
                nsCtrl: 'changePasswordCtrl',
                nsStatic: true,
            });
        };
        $scope.logout = function () {
            auth.logout();
        };
        // #endregion

        // #region Various
        $scope.showChat = function () {
            chat.show();
        };
        $scope.hideChat = function() {
            chat.hide();
        };

        $scope.url = function () {
            var url = $location.protocol() + "://" + $location.host() + ":" + $location.port();
            message.toastr(url);
        };

        $scope.video = function () {
            message.open({
                nsType: 'modal',
                nsSize: '640px',
                nsTemplate: '/partials/messages/explainerVideo.html'
            });
        }
        // #endregion
        // #endregion
    }
})();