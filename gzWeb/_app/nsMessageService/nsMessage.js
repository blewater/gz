(function () {
    'use strict';

    APP.directive('nsMessage', ['helpers', '$timeout', '$interval', '$controller', '$compile', '$templateRequest', '$window', 'message', '$rootScope', nsMessage]);

    function nsMessage(helpers, $timeout, $interval, $controller, $compile, $templateRequest, $window, message, $rootScope) {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                nsOptions: '=',
                nsIndex: '=',
                nsCount: '='
            },
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/nsMessageService/nsMessage.html');
            },
            link: function (scope, element, attrs) {
                // #region Variables
                scope.mobile = $rootScope.mobile;
                scope.now = new Date();
                var transitionDuration = 500;
                var intervalPromise, closePromise, hideTime;
                var unregisterCountWatch, unregisterMsgTitleHeightWatch, unregisterMsgBodyHeightWatch;
                var $msg = element.find('.msg');
                var $msgTitle = element.find('.msg-title');
                var $msgContent = element.find('.msg-content');
                var $msgBody = element.find('.msg-body');
                var widthClass;
                var templateCtrl;
                var templateScope = scope.$new();

                var oppositeTransitions = [];
                oppositeTransitions['show'] = 'hide';
                oppositeTransitions['enter-up'] = 'leave-down';
                oppositeTransitions['enter-down'] = 'leave-up';
                oppositeTransitions['enter-left'] = 'leave-right';
                oppositeTransitions['enter-right'] = 'leave-left';
                oppositeTransitions['slide-left-on'] = 'slide-right-off';
                oppositeTransitions['slide-right-on'] = 'slide-left-off';
                oppositeTransitions['slide-up-on'] = 'slide-down-off';
                oppositeTransitions['slide-down-on'] = 'slide-up-off';

                var sizeXLPadding = 40;
                var sizes = [];
                sizes['xs'] = 250;
                sizes['sm'] = 400;
                sizes['md'] = 800;
                sizes['lg'] = 1200;
                // #endregion

                // #region Methods
                scope.onBtnClick = function (key) {
                    scope.$broadcast('btnClicked', { key: key });
                };
                scope.nsCancel = function (reason) {
                    scope.nsOptions.nsReject(reason);
                    scope.close();
                };
                scope.nsOk = function (result) {
                    scope.nsOptions.nsResolve(result);
                    scope.close();
                };
                scope.nsNext = function (options) {
                    options.nsIn = 'slide-left-on';
                    options.nsOut = scope.nsOptions.nsOut;
                    scope.nsOptions.nsOut = 'slide-left-off';
                    scope.close();
                    var promise = message.open(options);
                    promise.then(scope.nsOptions.nsResolve, scope.nsOptions.nsReject);
                };
                scope.nsBack = function (options) {
                    options.nsIn = 'slide-right-on';
                    options.nsOut = scope.nsOptions.nsOut;
                    scope.nsOptions.nsOut = 'slide-right-off';
                    scope.close();
                    var promise = message.open(options);
                    promise.then(scope.nsOptions.nsResolve, scope.nsOptions.nsReject);
                };

                function attachBodyHtml(html) {
                    angular.forEach(scope.nsOptions.nsParams, function (value, key) {
                        templateScope[key] = value;
                    });
                    templateScope.nsCancel = scope.nsCancel;
                    templateScope.nsOk = scope.nsOk;
                    templateScope.nsNext = scope.nsNext;
                    templateScope.nsBack = scope.nsBack;
                    var ctrlId = scope.nsOptions.nsCtrl;

                    if (angular.isDefined(ctrlId))
                        templateCtrl = $controller(ctrlId, { $scope: templateScope });

                    if (html.length > 0)
                        $msgBody.html(html);
                    else
                        $msgBody.remove();

                    if (angular.isDefined(ctrlId))
                        $msgBody.children().data('$ngControllerController', templateCtrl);

                    $compile($msgBody.contents())(templateScope);
                }
                function init(options) {
                    scope.percentage = 100;

                    scope.nsOptions = options || scope.nsOptions;
                    scope.nsOptions.nsActive = false;

                    if (!angular.isDefined(scope.nsOptions.nsType))
                        scope.nsOptions.nsType = 'modal';
                    scope.isModal = scope.nsOptions.nsType === 'modal';
                    scope.isNotification = scope.nsOptions.nsType === 'notification';
                    scope.isToastr = scope.nsOptions.nsType === 'toastr';

                    if (!angular.isDefined(scope.nsOptions.nsClass))
                        scope.nsOptions.nsClass = 'custom';
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
                    if (!angular.isDefined(scope.nsOptions.nsTime))
                        scope.nsOptions.nsTime = scope.isNotification;

                    if (scope.isModal) {
                        if (!angular.isDefined(scope.nsOptions.nsIn))
                            scope.nsOptions.nsIn = 'enter-up';
                        if (!angular.isDefined(scope.nsOptions.nsOut))
                            scope.nsOptions.nsOut = oppositeTransitions[scope.nsOptions.nsIn];
                    }
                    else {
                        scope.nsOptions.nsIn = 'enter-up';
                        scope.nsOptions.nsOut = 'leave-down';
                    }

                    if (angular.isDefined(scope.nsOptions.nsTemplate))
                        $templateRequest(helpers.ui.getTemplate(scope.nsOptions.nsTemplate)).then(function(html) {
                            attachBodyHtml(html);
                        });
                    else
                        attachBodyHtml(scope.nsOptions.nsBody);

                    $msg.addClass('msg-' + scope.nsOptions.nsType);
                    $msg.addClass('msg-' + scope.nsOptions.nsClass);
                    $msg.addClass('msg-' + scope.nsOptions.nsIn);

                    function setWidth() {
                        var width = sizes[scope.nsOptions.nsSize];
                        if (!angular.isDefined(width) && scope.nsOptions.nsSize.indexOf('px') > -1)
                            width = scope.nsOptions.nsSize.slice(0, -2);

                        $msg.removeClass(widthClass);
                        $msg.removeClass('msg-xl');
                        $msg.css('width', '');

                        if (angular.isDefined(width) && width > angular.element($window).width() - sizeXLPadding)
                            $msg.addClass('msg-xl');
                        else {
                            if (['xs', 'sm', 'md', 'lg', 'xl'].indexOf(scope.nsOptions.nsSize) > -1) {
                                widthClass = 'msg-' + scope.nsOptions.nsSize;
                                $msg.addClass(widthClass);
                            }
                            else
                                $msg.css('width', scope.nsOptions.nsSize);
                        }
                    }
                    setWidth();

                    angular.element($window).bind('resize', function () {
                        setWidth();
                    });

                    unregisterCountWatch = scope.$watch('nsCount', function (newValue, oldValue) {
                        if (newValue < oldValue)
                            autoClose();
                    });
                    
                    scope.nsOptions.nsActive = true;
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
                    var modalZIndex = baseZIndex;
                    var notificationZIndex = 2 * baseZIndex;
                    var msgZIndex = scope.isModal ? modalZIndex : notificationZIndex;
                    return scope.isModal ? (msgZIndex + scope.nsIndex) : (msgZIndex - (scope.nsCount - scope.nsIndex));
                };
                scope.getOverlayZIndex = function () {
                    return scope.getMsgZIndex() - 1;
                };
                scope.showBadge = function () {
                    return scope.isNotification && scope.nsIndex > 0;
                };
                scope.showTime = function () {
                    return scope.isNotification && scope.nsTime === true;
                };
                scope.showClose = function () {
                    return scope.nsOptions.nsShowClose;
                };
                scope.showExpandCollapse = function () {
                    return scope.isNotification;
                };
                scope.isMinimized = function () {
                    return scope.isNotification && scope.$root.nsNotificationsMinimized;
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
                    $msg.removeClass('msg-' + scope.nsOptions.nsIn);
                    $msg.addClass('msg-' + scope.nsOptions.nsOut);

                    var $nextMsgBody = angular.element('#ns-notifications').find('.msg-container').eq(scope.nsIndex - 1).find('.msg');
                    if (scope.nsCount > 1)
                        $nextMsgBody.addClass('msg-preparing');

                    scope.nsOptions.nsActive = false;
                    closePromise = $timeout(function () {
                        //cancelInterval();

                        $nextMsgBody.removeClass('msg-preparing');
                        if (scope.isModal)
                            scope.$root.nsModals.splice(scope.nsIndex, 1);
                        else if (scope.isNotification)
                            scope.$root.nsNotifications.splice(scope.nsIndex, 1);
                        else
                            scope.$root.nsToastrs.splice(scope.nsIndex, 1);

                        if (angular.isFunction(scope.nsOptions.nsCallback))
                            scope.nsOptions.nsCallback();
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