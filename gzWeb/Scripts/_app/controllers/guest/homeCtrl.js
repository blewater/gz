(function () {
    'use strict';
    var ctrlId = 'homeCtrl';
    APP.controller(ctrlId, ['$scope', 'message', ctrlFactory]);
    function ctrlFactory($scope, message) {
        var m = 1, n = 1, t = 1;
        $scope.modal = function () {
            message.error('Error' + m++);
        };
        $scope.notify = function () {
            message.notify('Notification ' + n++);
        };
        $scope.toastr = function () {
            message.toastr('Toastr ' + t++);
        };
    }
})();