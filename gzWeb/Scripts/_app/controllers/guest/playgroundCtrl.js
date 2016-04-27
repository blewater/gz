(function () {
    'use strict';
    var ctrlId = 'playgroundCtrl';
    APP.controller(ctrlId, ['$scope', 'message', 'emWamp', ctrlFactory]);
    function ctrlFactory($scope, message, emWamp) {
        // #region Playground
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
            message.toastr('Toastr ' + t++, {
                nsAutoCloseDelay: 50000
            });
        };

        $scope.registerStart = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerStart.html',
                nsCtrl: 'registerStartCtrl',
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
        $scope.logout = function () {
            emWamp.logout();
        };
        // #endregion
    }
})();