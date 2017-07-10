(function () {
    'use strict';

    APP.directive('gzCarousel', ['helpers', '$timeout', '$window', '$log', 'message', directiveFactory]);

    function directiveFactory(helpers, $timeout, $window, $log, message) {
        return {
            restrict: 'E',
            scope: {
                gzSlides: '='
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/games/gzCarousel.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                //var IMG_WIDTH = 1920;
                //var IMG_HEIGHT = 1200;
                //var aspect = IMG_WIDTH / IMG_HEIGHT;

                //angular.element($window).bind('resize', function () {
                //    $log.error(($window.innerWidth * IMG_HEIGHT) / IMG_WIDTH);
                //});                
                //$scope.calcHeight = function () {
                //    var bigHeigth = (windowWidth * IMG_HEIGHT) / IMG_WIDTH;
                //    $log.error()
                //};

                var DELAY = 10000;
                var timeoutPromise = undefined;
                var watchingVideo = false;
                $scope.currentIndex = 0;
                checkShowControlsAndIndicators();

                function normalizeIndex(index) {
                    return (index + $scope.gzSlides.length) % $scope.gzSlides.length;
                }

                function autoplay(index) {
                    timeoutPromise = $timeout(function () {
                        setSlide(index + 1);
                    }, DELAY);
                }

                function setSlide(index) {
                    $scope.currentIndex = normalizeIndex(index);
                    autoplay(index);
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

                function pause() {
                    if (angular.isDefined(timeoutPromise))
                        $timeout.cancel(timeoutPromise);
                };
                function resume() {
                    autoplay($scope.currentIndex);
                }

                $scope.onHover = function () {
                    pause();
                };
                $scope.onLeave = function () {
                    if (!watchingVideo)
                        resume();
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
                        checkShowControlsAndIndicators();
                        unregisterCollectionWatch();
                    }
                });
                $scope.watchVideo = function (url) {
                    watchingVideo = true;
                    pause();
                    var videoPromise = message.open({
                        nsType: 'modal',
                        nsSize: '800px',
                        nsTemplate: '_app/games/carouselVideo.html',
                        nsCtrl: 'carouselVideoCtrl',
                        nsParams: { url: url },
                        nsStatic: true
                    });
                    videoPromise.then(function () {
                        watchingVideo = false;
                        resume();
                    });
                }


                $scope.$on("$destroy", function (event) {
                    pause();
                });

                function checkShowControlsAndIndicators() {
                    $scope.showControls = !('gzNoControls' in $attrs) && $scope.gzSlides && $scope.gzSlides.length > 1;
                    $scope.showIndicators = !('gzNoIndicators' in $attrs) && $scope.gzSlides && $scope.gzSlides.length > 1;
                }
            }]
        };
    }
})();