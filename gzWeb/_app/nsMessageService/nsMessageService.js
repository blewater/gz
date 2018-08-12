(function () {
    'use strict';

    APP.factory('message', ['$rootScope', '$compile', '$q', 'chat', serviceFactory]);
    function serviceFactory($rootScope, $compile, $q, chat) {
        clear();

        //var bodyElement = angular.element(document.querySelector('body'));
        //var messagesElement = $compile('<ns-messages ns-modals="nsModals" ns-notifications="nsNotifications" ns-toastrs="nsToastrs"></ns-messages>')($rootScope);
        //bodyElement.prepend(messagesElement);

        var services = {
            clear: clear,
            open: open,
            modal: modal,
            notify: notify,
            toastr: toastr,
            error: error,
            autoCloseError: autoCloseError,
            success: success,
            warning: warning,
            info: info,
            prompt: prompt,
            confirm: confirm,
            alert: alert
        };
        return services;

        function clear() {
            $rootScope.nsModals = [];
            $rootScope.nsNotifications = [];
            $rootScope.nsToastrs = [];
        }

        /**
         * Opens a message
         * @param {JSON} options
         *      option: data type (description - default)

         *      nsSize: 'xs' || 'sm' || 'md' || 'lg' || 'xl' || custom (message box size - 'sm')
         *      nsClass: 'custom' || 'success' || 'error' || 'warning' || 'info' || 'prompt' (styling class - 'custom')
         *      nsType: 'modal' || 'notification' || 'toastr' (message box type - 'modal')
         *      nsIn: 'show' || 'enter-up' || 'enter-down' || 'enter-left' || 'enter-right' || 'slide-up-on' || 'slide-down-on' || 'slide-left-on' || 'slide-right-on' (modals opening transition - 'enter-up')
         *      nsOut: 'hide' || 'leave-down' || 'leave-up' || 'leave-right' || 'leave-left' || 'slide-down-off' || 'slide-up-off' || 'slide-right-off' || 'slide-left-off' (modals closing transition - 'leave-down')
         *      nsRoute: string (route - '')
         *      nsTitle: string (title of the message - '')
         *      nsTitleShout: boolean (should title be emphatic - false)
         *      nsBody: string (plain text as message body, in use when no template is being specified - '')
         *      nsTemplate: string (message body as template url - '')
         *      nsCtrl: string (id of the controller associated with template - '')
         *      nsParams: JSON (parameters to pass to controller - {})
         *      nsIconClass: string (title icon class - '')
         *      nsIconClassInversed: boolean (inverse icon colors - false)
         *      nsCallback: function (function to execute upon close - angular.noop)
         *      nsShowClose: boolean (should show close icon - true)
         *      nsTime: boolean (should show time - true when nsType IS 'notification' else false)
         *      nsAutoClose: boolean (should auto close - true when nsType IS 'toastr' else false)
         *      nsAutoCloseDelay: number (milliseconds to wait before close - 5000)
         *      nsCloseOnTitleClick: boolean (true when nsType IS NOT 'modal' else false)
         *      nsBackdrop: boolean (should show backdrop - true when nsType IS 'modal' else false)
         *      nsStatic: boolean (should not to be closed upon clicking outside - true when nsType IS 'notification' else false)
         *      nsPromptButtons: JSON array (buttons to show - true when nsType IS 'modal' else false)

         * @return {Promise} open
         */
        function open(options) {
            options = options || {};

            var deferred = $q.defer();
            var promise = deferred.promise;
            angular.extend(options, {
                nsResolve: deferred.resolve,
                nsReject: deferred.reject
            });

            if (options.nsType === 'toastr')
                //$rootScope.nsToastrs.splice(0, 0, options);
                $rootScope.nsToastrs.push(options);
            else if (options.nsType === 'notification')
                //$rootScope.nsNotifications.splice(0, 0, options);
                $rootScope.nsNotifications.push(options);
            else
                $rootScope.nsModals.push(options);

            return promise;
        }

        function modal(msg, options) {
            var defaults = {
                nsType: 'modal',
                nsClass: 'default',
                nsTitle: msg
            };
            return open(angular.extend(defaults, options));
        }
        function notify(msg, options) {
            chat.min();
            var defaults = {
                nsType: 'notification',
                nsClass: 'default',
                nsTitle: msg && msg.Message ? msg.Message : msg
            };
            return open(angular.extend(defaults, options));
        }
        function toastr(msg, options) {
            chat.min();
            var defaults = {
                nsType: 'toastr',
                nsClass: 'default',
                nsTitle: msg && msg.Message ? msg.Message : msg
            };
            return open(angular.extend(defaults, options));
        }

        function error(msg, options) {
            //function createErrorMsg(err) {
            //    if (err && err.Message) {
            //        var errorElement = angular.element('<div></div>');
            //        var errorMessageElement = angular.element('<h5></h5>');
            //        errorMessageElement.append(err.Message);
            //        errorElement.append(errorMessageElement);
            //        if (err.MessageDetail) {
            //            var errorMessageDetailElement = angular.element('<h6></h6>');
            //            errorMessageDetailElement.append(err.MessageDetail);
            //            errorElement.append(errorMessageDetailElement);
            //        }
            //        return errorElement[0].outerHTML;
            //    }
            //    else
            //        return err;
            //}
            var defaults = {
                nsClass: 'error',
                nsTitle: msg,
                nsIconClass: 'fa-times'
            };
            return notify(msg, angular.extend(defaults, options));
        }
        function autoCloseError(msg) {
            return error(msg, { nsType: 'toastr', nsAutoCloseDelay: 20000 });
        }
        function success(msg, options) {
            var defaults = {
                nsClass: 'success',
                nsTitle: msg,
                nsIconClass: 'fa-check'
            };
            return notify(msg, angular.extend(defaults, options));
        }
        function warning(msg, options) {
            var defaults = {
                nsClass: 'warning',
                nsTitle: msg,
                nsIconClass: 'fa-exclamation-triangle'
            };
            return notify(msg, angular.extend(defaults, options));
        }
        function info(msg, options) {
            var defaults = {
                nsClass: 'info',
                nsTitle: msg,
                nsIconClass: 'fa-bell'
            };
            return notify(msg, angular.extend(defaults, options));
        }

        function prompt(msg, okCallBack, cancelCallback, options) {
            var defaults = {
                nsCtrl: 'nsPromptCtrl',
                nsClass: 'prompt',
                nsIconClass: 'fa-question',
                nsSize: 'md',
                nsStatic: true,
                nsPromptButtons: [
                    { text: 'OK', eventKey: "ok" }
                ]
            };
            var promise = modal(msg, angular.extend(defaults, options));
            promise.then(okCallBack, cancelCallback);
        }
        function confirm(msg, okCallBack, cancelCallback, options) {
            var defaults = {
                nsTitle: msg,
                nsCtrl: 'nsConfirmCtrl',
                nsPromptButtons: [
                    { text: 'Cancel', eventKey: "cancel" },
                    { text: 'OK', eventKey: "ok" }
                ]
            };
            prompt(msg, okCallBack, cancelCallback, angular.extend(defaults, options));
        }
        function alert(msg, okCallBack, options) {
            var defaults = {
                nsIconClass: 'fa-exclamation'
            };
            prompt(msg, okCallBack, angular.noop, angular.extend(defaults, options));
        }
    };
})();

