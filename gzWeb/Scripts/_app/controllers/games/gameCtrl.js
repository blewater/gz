(function () {
    'use strict';
    var ctrlId = 'gameCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$routeParams', '$sce', 'emCasino', '$window', '$interval', '$location', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $controller, $routeParams, $sce, emCasino, $window, $interval, $location, constants) {
        $controller('authCtrl', { $scope: $scope });

        $scope.game = null;
        $scope.gameLaunchData = null;
        $scope.gameUrl = null;
        $scope.playForRealMoney = true;        

        function loadGame() {
            var slug = $routeParams.slug;
            emCasino.getGames({
                filterBySlug: [slug],
                filterByPlatform: null,
                expectedFields:
                    emCasino.FIELDS.Slug +
                    emCasino.FIELDS.Name +
                    emCasino.FIELDS.BackgroundImage +
                    emCasino.FIELDS.License +
                    emCasino.FIELDS.Width + 
                    emCasino.FIELDS.Height,
                pageSize: 1,
            }).then(function (gamesResult) {
                $scope.game = gamesResult.games[0];

                function setGameDimensions() {
                    var percent = 0.6;
                    var windowWidth = $window.innerWidth;
                    var windowHeight = $window.innerHeight - 50;
                    var windowAspect = windowWidth / windowHeight;
                    var gameAspect = $scope.game.width / $scope.game.height;
                    if (windowAspect >= 1) {
                        $scope.gameHeight = Math.round(windowHeight * percent);
                        $scope.gameWidth= Math.round($scope.gameHeight * gameAspect);
                    }
                    else {
                        $scope.gameWidth = Math.round(windowWidth * percent);
                        $scope.gameHeight = Math.round($scope.gameWidth / gameAspect);
                    }
                }

                setGameDimensions();
                angular.element($window).bind('resize', function () {
                    setGameDimensions();
                });
                
                emCasino.getLaunchUrl($scope.game.slug, null, $scope.playForRealMoney).then(function (launchDataResult) {
                    $scope.gameLaunchData = launchDataResult;
                    $scope.gameUrl = $sce.trustAsResourceUrl(launchDataResult.url);
                }, logError);
            }, logError);
        }

        $scope._init('game', function () {
            loadGame();
            setClock();
        });

        function setClock() {
            var tickInterval = 1000;
            var tick = function () { $scope.clock = Date.now(); }
            $interval(tick, tickInterval);
            tick();
        }

        $scope.isFullscreen = false;
        $scope.toggleFullScreen = function () {
            $scope.isFullscreen = !$scope.isFullscreen;
        }

        $scope.backToGames = function () {
            $location.path(constants.routes.games.path);
        }


        function logError(error) {
            console.log(error);
        };
    }
})();
