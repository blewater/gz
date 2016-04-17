(function () {
    'use strict';

    APP.directive('nsMessage', ['helpers', '$timeout', '$interval', '$controller', '$compile', '$templateRequest', nsMessage]);

    function nsMessage(helpers, $timeout, $interval, $controller, $compile, $templateRequest) {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                nsOptions: '=',
                nsIndex: '=',
                nsCount: '='
            },
            templateUrl: function () {
                return helpers.ui.getTemplate('scripts/_app/services/nsMessagesService/nsMessage.html');
            },
            link: function (scope, element, attrs) {
                // #region Variables
                var transitionDuration = 500;
                var intervalPromise, closePromise, hideTime;
                var unregisterCountWatch, unregisterMsgTitleHeightWatch, unregisterMsgBodyHeightWatch;
                var $msg = element.find('.msg');
                var $msgTitle = element.find('.msg-title');
                var $msgBody = element.find('.msg-body');
                var templateCtrl;
                var templateScope = scope.$new();
                // #endregion

                // #region Methods
                scope.onBtnClick = function (btn) {
                    scope.$broadcast('btnClicked', { key: btn.eventKey });
                };
                scope.nsCancel = function (reason) {
                    scope.nsOptions.nsReject(reason);
                    scope.close();
                };
                scope.nsOk = function (result) {
                    scope.nsOptions.nsResolve(result);
                    scope.close();
                };

                function attachBodyHtml(html) {
                    angular.forEach(scope.nsOptions.nsParams, function (value, key) {
                        templateScope[key] = value;//.toString();
                    });
                    templateScope.nsCancel = scope.nsCancel;
                    templateScope.nsOk = scope.nsOk;
                    var ctrlId = scope.nsOptions.nsCtrl;

                    if (angular.isDefined(ctrlId))
                        templateCtrl = $controller(scope.nsOptions.nsCtrl, { $scope: templateScope });

                    if (html.length > 0)
                        $msgBody.html(html);
                    else
                        $msgBody.remove();

                    if (angular.isDefined(ctrlId))
                        $msgBody.children().data('$ngControllerController', templateCtrl);

                    $compile($msgBody.contents())(templateScope);
                }
                function init() {
                    scope.percentage = 100;
                    scope.nsOptions.nsActive = false;
                    if (!angular.isDefined(scope.nsOptions.nsType))
                        scope.nsOptions.nsType = 'modal';
                    scope.isModal = scope.nsOptions.nsType === 'modal';
                    scope.isNotification = scope.nsOptions.nsType === 'notification';
                    scope.isToastr = scope.nsOptions.nsType === 'toastr';

                    if (!angular.isDefined(scope.nsOptions.nsClass))
                        scope.nsOptions.nsClass = 'default';
                    if (!angular.isDefined(scope.nsOptions.nsBody))
                        scope.nsOptions.nsBody = '';
                    //if (!angular.isDefined(scope.nsOptions.nsBodyType))
                    //    scope.nsOptions.nsBodyType = 'html';
                    if (!angular.isDefined(scope.nsOptions.nsTitle))
                        scope.nsOptions.nsTitle = '';
                    if (!angular.isDefined(scope.nsOptions.nsTitleShout))
                        scope.nsOptions.nsTitleShout = scope.isModal;
                    if (!angular.isDefined(scope.nsOptions.nsIconClass)) {
                        switch (scope.nsOptions.nsClass) {
                            case 'error': scope.nsOptions.nsIconClass = 'fa-times'; break;
                            case 'success': scope.nsOptions.nsIconClass = 'fa-check'; break;
                            case 'warning': scope.nsOptions.nsIconClass = 'fa-exclamation-triangle'; break;
                            case 'info': scope.nsOptions.nsIconClass = 'fa-bell'; break;
                            case 'prompt': scope.nsOptions.nsIconClass = 'fa-question'; break;
                            default: scope.nsOptions.nsIconClass = ''; break;
                        }
                    }
                    if (!angular.isDefined(scope.nsOptions.nsIconClassInversed))
                        scope.nsOptions.nsIconClassInversed = false;
                    if (!angular.isDefined(scope.nsOptions.nsCallback))
                        scope.nsOptions.nsCallback = angular.noop;
                    if (!angular.isDefined(scope.nsOptions.nsShowClose))
                        scope.nsOptions.nsShowClose = true;//scope.isModal;
                    if (!angular.isDefined(scope.nsOptions.nsAutoClose))
                        scope.nsOptions.nsAutoClose = scope.isToastr;
                    if (!angular.isDefined(scope.nsOptions.nsAutoCloseDelay))
                        scope.nsOptions.nsAutoCloseDelay = 5000;
                    if (!angular.isDefined(scope.nsOptions.nsCloseOnTitleClick))
                        scope.nsOptions.nsCloseOnTitleClick = !scope.isModal;
                    if (!angular.isDefined(scope.nsOptions.nsBackdrop))
                        scope.nsOptions.nsBackdrop = scope.isModal;
                    if (!angular.isDefined(scope.nsOptions.nsStatic))
                        scope.nsOptions.nsStatic = scope.isNotification;
                    if (!angular.isDefined(scope.nsOptions.nsSize))
                        scope.nsOptions.nsSize = 'sm';

                    if (angular.isDefined(scope.nsOptions.nsTemplate))
                        $templateRequest(helpers.ui.getTemplate(scope.nsOptions.nsTemplate)).then(function(html) {
                            attachBodyHtml(html);
                        });
                    else
                        attachBodyHtml(scope.nsOptions.nsBody);

                    $msg.addClass('msg-' + scope.nsOptions.nsType);
                    $msg.addClass('msg-' + scope.nsOptions.nsClass);
                    //$msg.addClass(scope.nsOptions.nsStatic ? 'msg-static' : 'msg-non-static');

                    if (['xs', 'sm', 'md', 'lg', 'xl'].indexOf(scope.nsOptions.nsSize) > -1)
                        $msg.addClass('msg-' + scope.nsOptions.nsSize);
                    else
                        $msg.css('width', scope.nsOptions.nsSize);

                    //registerHeightWatches();

                    unregisterCountWatch = scope.$watch('nsCount', function (newValue, oldValue) {
                        if (newValue < oldValue)
                            autoClose();
                    });
                    
                    $timeout(function () { scope.nsOptions.nsActive = true; }, 0);
                    autoClose();
                };

                function autoClose() {
                    if (scope.nsOptions.nsAutoClose && !hideTime) {
                        var hasFocus = scope.nsIndex === scope.nsCount - 1;
                        if ((scope.isModal && hasFocus) || scope.isToastr)
                            $timeout(countdown, 0);
                    }
                }

                function registerHeightWatches() {
                    var windowHeight = window.innerHeight;
                    //unregisterMsgTitleHeightWatch = scope.$watch(function () {
                    //    return $msgTitle.height();
                    //}, function (newValue, oldValue) {
                    //    if (newValue > 0) {
                    //        scope.height = $msg.height();
                    //    }
                    //});
                    unregisterMsgBodyHeightWatch = scope.$watch(function () {
                        return $msgBody.height();
                    }, function (newValue, oldValue) {
                        if (newValue > 0) {
                            $msg.css('height', newValue / windowHeight >= 0.8 ? '100%' : 'auto');
                            //$msg.css('height', '100%');
                            scope.height = $msg.height();
                            //unregisterMsgBodyHeightWatch();
                        }
                    });
                }
                function resetCountdown() {
                    //console.log('resetCountdown');
                    scope.percentage = 100;
                    hideTime = new Date().getTime() + scope.nsOptions.nsAutoCloseDelay;
                    cancelInterval();
                }
                function cancelInterval() {
                    //console.log('cancel');
                    if (angular.isDefined(intervalPromise)) {
                        $interval.cancel(intervalPromise);
                        intervalPromise = undefined;
                        hideTime = undefined;
                        //console.log('cancelled');
                    }

                };
                function countdown() {
                    //console.log('countdown');
                    cancelInterval();
                    hideTime = new Date().getTime() + scope.nsOptions.nsAutoCloseDelay;
                    intervalPromise = $interval(function () {
                        if (hideTime) {
                            var newPercentage = ((hideTime - new Date().getTime()) / scope.nsOptions.nsAutoCloseDelay) * 100;
                            scope.percentage = newPercentage;
                            //console.log(scope.nsIndex + ': ' + scope.percentage);
                            if (scope.percentage <= 0) {
                                cancelInterval();
                                scope.close();
                            }
                        }
                    }, 10);
                }
                // #endregion

                // #region scope Methods
                scope.getMsgZIndex = function () {
                    var baseZIndex = 1000000;
                    var modalZIndex = 2* baseZIndex;
                    var notificationZIndex = baseZIndex;
                    var msgZIndex = scope.isModal ? modalZIndex : notificationZIndex;
                    return scope.isModal ? (msgZIndex + scope.nsIndex) : (msgZIndex - (scope.nsCount - scope.nsIndex));
                };
                scope.getOverlayZIndex = function () {
                    return scope.getMsgZIndex() - 1;
                };
                scope.showBadge = function () {
                    return scope.isNotification && scope.nsIndex > 0;
                };
                scope.showClose = function () {
                    return scope.nsOptions.nsShowClose;
                };
                scope.showExpandCollapse = function () {
                    return scope.isNotification;
                };
                scope.isMinimized = function () {
                    return scope.$root.nsNotificationsMinimized;
                };
                scope.toggleMinMax = function () {
                    scope.$root.nsNotificationsMinimized = !scope.$root.nsNotificationsMinimized;
                };

                scope.onOverlayClick = function () {
                    if (!scope.nsOptions.nsStatic)
                        scope.nsCancel('ignore');
                };
                scope.onCloseClick = function () {
                    scope.nsCancel('close');
                };
                scope.onTitleClick = function () {
                    if (scope.nsOptions.nsCloseOnTitleClick)
                        scope.nsCancel('close');
                };

                scope.close = function () {
                    //console.log('close');
                    var $nextMsgBody = $('#ns-notifications').find('.msg-container').eq(scope.nsIndex - 1).find('.msg');
                    if (scope.nsCount > 1) {
                        $nextMsgBody.addClass('msg-preparing');
                    }

                    $timeout(function () { scope.nsOptions.nsActive = false; }, 0);
                    closePromise = $timeout(function () {
                        //cancelInterval();

                        $nextMsgBody.removeClass('msg-preparing');
                        if (scope.isModal)
                            scope.$root.nsModals.pop();
                        else if (scope.isNotification)
                            scope.$root.nsNotifications.splice(scope.nsIndex, 1);
                        else
                            scope.$root.nsToastrs.splice(scope.nsIndex, 1);

                        helpers.reflection.checkAndInvokeFunction(scope.nsOptions.nsCallback);
                        //console.log('remove');
                    }, transitionDuration);
                }
                scope.onMouseEnter = function () {
                    //if (scope.nsOptions.nsAutoClose) {
                    //    if (scope.isNotification)
                    //        countdown();
                    //    else
                    //        resetCountdown();
                    //}
                };
                scope.onMouseLeave = function () {
                    //if (scope.nsOptions.nsAutoClose) {
                    //    if (scope.isNotification)
                    //        resetCountdown();
                    //    else
                    //        countdown();
                    //}
                };
                scope.$on('$destroy', function () {
                    cancelInterval();
                    unregisterCountWatch();
                    //unregisterMsgTitleHeightWatch();
                    //unregisterMsgBodyHeightWatch();
                });
                // #endregion

                init();
            }
        };
    }
})();