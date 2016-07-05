(function () {
    'use strict';

    APP.directive('gzCarousel', ['helpers', '$timeout', '$interval', directiveFactory]);

    function directiveFactory(helpers, $timeout, $interval) {
        return {
            restrict: 'E',
            scope: {
                gzSlides: '='
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('partials/directives/gzCarousel.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                var TRANSITION_DELAY = 3000;
                var WAIT_DELAY = 5000;
                var timeoutPromise = undefined;
                var intervalPromise = undefined;
                var transitionTime = null, waitTime = null;
                $scope.currentIndex = 0;
                $scope.showControls = !('gzNoControls' in $attrs);
                $scope.showIndicators = !('gzNoIndicators' in $attrs);
                $scope.showProgress = !('gzNoProgress' in $attrs);
                
                function normalizeIndex(index) {
                    return (index + $scope.gzSlides.length) % $scope.gzSlides.length;
                }

                function setSlide(index) {
                    $scope.currentIndex = normalizeIndex(index);
                    timeoutPromise = $timeout(function () {
                        setSlide(index + 1);
                    }, TRANSITION_DELAY + WAIT_DELAY);

                    transitionTime = new Date().getTime() + TRANSITION_DELAY;
                    waitTime = new Date().getTime() + TRANSITION_DELAY + WAIT_DELAY;
                    if (!angular.isDefined(intervalPromise)) {
                        intervalPromise = $interval(function () {
                            var now = new Date().getTime();
                            $scope.progress =
                                now < transitionTime
                                ? ((transitionTime - now) / TRANSITION_DELAY) * 100
                                : 100 - ((waitTime - now) / WAIT_DELAY) * 100;
                        }, 10);
                    }
                }

                $scope.gotoNext = function () {
                    $scope.gotoSlide($scope.currentIndex + 1);
                };
                $scope.gotoPrevious = function () {
                    $scope.gotoSlide($scope.currentIndex - 1);
                };
                $scope.gotoSlide = function (index) {
                    if (angular.isDefined(timeoutPromise))
                        $timeout.cancel(timeoutPromise);
                    setSlide(index);
                }

                $scope.onHover = function () {
                    if (angular.isDefined(timeoutPromise))
                        $timeout.cancel(timeoutPromise);
                };
                $scope.onLeave = function () {
                    timeoutPromise = $timeout(function () {
                        setSlide($scope.currentIndex + 1);
                    }, TRANSITION_DELAY + WAIT_DELAY);
                };

                $scope.calcMarginLeft = function (index) {
                    return ((index - $scope.currentIndex) * 100) + '%';
                }
                $scope.calcZIndex = function (index) {
                    var diff = index - $scope.currentIndex;
                    var z = -1;
                    if (diff === 0)
                        z = 2;
                    else if (diff === 1 || diff === 1 - $scope.gzSlides.length)
                        z = 1;
                    else if (diff === -1 || diff === $scope.gzSlides.length - 1)
                        z = 1;
                    return z;
                }

                var unregisterCollectionWatch = $scope.$watchCollection('gzSlides', function (newCollection, oldCollection) {
                    if (newCollection && newCollection.length > 0) {
                        setSlide(0);
                        unregisterCollectionWatch();
                    }
                });

                $scope.$on("$destroy", function (event) {
                    if (angular.isDefined(timeoutPromise))
                        $timeout.cancel(timeoutPromise);
                    if (angular.isDefined(intervalPromise))
                        $interval.cancel(intervalPromise);
                });
            }]
        };
    }
})();