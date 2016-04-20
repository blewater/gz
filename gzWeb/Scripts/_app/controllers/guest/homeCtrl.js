(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', 'message', ctrlFactory]);
    function ctrlFactory($scope, message) {
        // #region Playground
        //var m = 1, n = 1, t = 1;
        //$scope.alert = function () {
        //    message.alert('Alert ' + m++);
        //};
        //$scope.confirm = function () {
        //    message.confirm('Confirm ' + m++);
        //};
        //$scope.prompt = function () {
        //    message.prompt('Prompt' + m++);
        //};
        //$scope.notify = function () {
        //    message.notify('Notification ' + n++);
        //};
        //$scope.toastr = function () {
        //    message.toastr('Toastr ' + t++);
        //};

        $scope.register = function () {
            var promise =
                message.open({
                    nsType: 'modal',
                    nsSize: 'md',
                    nsTemplate: '/partials/messages/register.html',
                    nsCtrl: 'registerCtrl',
                    nsStatic: true,
                });
            promise.then(function(result) {
                
            });
        };
        $scope.login = function () {
            var promise =
                message.open({
                    nsType: 'modal',
                    nsSize: 'sm',
                    nsTemplate: '/partials/messages/login.html',
                    nsCtrl: 'loginCtrl',
                    nsStatic: true,
                });
            promise.then(function(result) {
                
            });
        };
        $scope.forgot = function () {
            var promise =
                message.open({
                    nsType: 'modal',
                    nsSize: 'sm',
                    nsTemplate: '/partials/messages/forgotPassword.html',
                    nsCtrl: 'forgotPasswordCtrl',
                    nsStatic: true,
                });
            promise.then(function (result) {

            });
        };
        $scope.logout = function () {
        };
        // #endregion
    }
})();