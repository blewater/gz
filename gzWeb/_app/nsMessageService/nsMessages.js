(function () {
    'use strict';

    APP.directive('nsMessages', ['helpers', nsMessages]);

    function nsMessages(helpers) {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                nsModals: '=',
                nsNotifications: '=',
                nsToastrs: '='
            },
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/nsMessageService/nsMessages.html');
            },
            link: function (scope, element, attrs) {
                scope.getToastrsBottom = function() {
                    var margin = 10;
                    var $notifications = $('#ns-notifications').find('.msg').last();
                    var i, bottom = 35;
                    for (i = 0; i < $notifications.length; i++)
                        bottom += (margin + $notifications[i].offsetHeight);
                    return bottom;
                }
            }
        };
    }
})();