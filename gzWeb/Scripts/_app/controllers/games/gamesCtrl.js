(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'iso4217', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, iso4217, helpers) {
        $controller('authCtrl', { $scope: $scope });

        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.spinnerGreenXs = constants.spinners.xs_rel_green;
        $scope.spinnerWhiteXs = constants.spinners.xs_rel_white;
        var i = 0;
        var pageSize = 10;
        $scope.playForRealMoney = true;

        // #region Featured Games
        var featuredGameFields = emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity + emCasino.FIELDS.Logo + emCasino.FIELDS.BackgroundImage;
        
        function getMostPopularGames() {
            $scope.mostPopularGames = [];
            var parameters = {
                expectedFields: featuredGameFields,
                expectedFormat: 'array',
                filterByPlatform: null,
                pageIndex: 1,
                pageSize: 5,
                sortFields: [{ field: emCasino.FIELDS.Popularity, order: 'DESC' }]
            };
            emCasino.getGames(parameters).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j];
                    game.info = "#" + (j + 1);
                    $scope.mostPopularGames.push(game);
                }
            }, logError);
        }

        function getRecommendedGames() {
            $scope.recommendedGames = [];
            var parameters = {
                recommendedType: "user",
                platform: null,
                expectedGameFields: featuredGameFields
            };
            emCasino.getRecommendedGames(parameters).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var gameObj = result.games[j];
                    var game = gameObj.game;
                    game.info = "# " + gameObj.rankId;
                    $scope.recommendedGames.push(game);
                }
            }, logError);
        }

        function getBiggestWinGames() {
            $scope.biggestWinGames = [];
            emCasino.getBiggestWinGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var gameObj = result.games[j];
                    var game = gameObj.game;
                    game.info = iso4217.getCurrencyByCode(gameObj.postingCurrency).symbol + " " + $filter('number')(gameObj.postingAmount, 2);
                    $scope.biggestWinGames.push(game);
                }
            }, logError);
        }

        function getLastPlayedGames() {
            $scope.lastPlayedGames = [];
            emCasino.getLastPlayedGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var gameObj = result.games[j];
                    var game = gameObj.game;
                    game.info = $filter('date')(gameObj.lastPlayed, 'short');
                    $scope.lastPlayedGames.push(game);
                }
            }, logError);
        }

        function getMostPlayedGames() {
            $scope.mostPlayedGames = [];
            emCasino.getMostPlayedGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var gameObj = result.games[j];
                    var game = gameObj.game;
                    game.info = gameObj.gameRounds;
                    $scope.mostPlayedGames.push(game);
                }
            }, logError);
        }

        function getJackpots() {
            $scope.jackpots = [];
            var parameters = {
                expectedGameFields: featuredGameFields
            };
            emCasino.getJackpots(parameters).then(function (result) {
                for (var j = 0; j < result.jackpots.length; j++) {
                    var jackpotObj = result.jackpots[j];
                    var game = jackpotObj.game;
                    game.info = iso4217.getCurrencyByCode($scope._authData.currency).symbol + " " + jackpotObj.amount[$scope._authData.currency];
                    $scope.jackpots.push(game);
                }
            }, logError);
        }

        function getFeaturedGames() {
            getMostPopularGames();
            getRecommendedGames();
            //getBiggestWinGames();
            //getLastPlayedGames();
            //getMostPlayedGames();
            //getJackpots();
        };
        // #endregion

        // #region SlideShow
        //var INTERVAL = 10000;
        //var currentIndex = 0;
        //var timeoutPromise = undefined;
        ////var intervalPromise = undefined;
        ////var slideTime = null;

        //function initSlides() {
        //    for (var j = 0; j < $scope.featuredGames.length; j++) {
        //        $scope.featuredGames[j].isCurrent = false;
        //        $scope.featuredGames[j].isPrev = false;
        //        $scope.featuredGames[j].isNext = false;
        //    }
        //}
        //function normalizeIndex(index) {
        //    return (index + $scope.featuredGames.length) % $scope.featuredGames.length;
        //}
        //function setCurrentSlide(index) {
        //    initSlides();
        //    $scope.featuredGames[index].isCurrent = true;
        //    $scope.featuredGames[normalizeIndex(index - 1)].isPrev = true;
        //    $scope.featuredGames[normalizeIndex(index + 1)].isNext = true;
        //}
        //function slideShow(index, mode) {
        //    currentIndex = index;
        //    setCurrentSlide(index);
        //    $scope.slideMode = mode;
        //    timeoutPromise = $timeout(function () {
        //        slideShow(normalizeIndex(index + 1), 'auto');
        //    }, INTERVAL);

        //    //slideTime = new Date().getTime() + INTERVAL;
        //    //if (!angular.isDefined(intervalPromise)) {
        //    //    intervalPromise = $interval(function () {
        //    //        if (slideTime) {
        //    //            $scope.slideProgress = ((slideTime - new Date().getTime()) / INTERVAL) * 100;
        //    //        }
        //    //    }, 10);
        //    //}
        //}

        //$scope.onNext = function () {
        //    $scope.gotoSlide(normalizeIndex(currentIndex + 1));
        //};
        //$scope.onPrev = function () {
        //    $scope.gotoSlide(normalizeIndex(currentIndex - 1));
        //};
        //$scope.gotoSlide = function(index) {
        //    if (angular.isDefined(timeoutPromise))
        //        $timeout.cancel(timeoutPromise);
        //    slideShow(index, 'manual');
        //}
        // #endregion

        // #region Filtered Games
        $scope.getCategoryTitle = function (name) {
            switch (name) {
                case "VIDEOSLOTS": return "VIDEO SLOTS";
                case "JACKPOTGAMES": return "JACKPOT GAMES";
                case "MINIGAMES": return "MINI GAMES";
                case "CLASSICSLOTS": return "CLASSIC SLOTS";
                case "LOTTERY": return "LOTTERY";
                case "VIDEOPOKERS": return "VIDEO POKERS";
                case "TABLEGAMES": return "TABLE GAMES";
                case "SCRATCHCARDS": return "SCRATCH CARDS";
                case "3DSLOTS": return "3D SLOTS";
                case "LIVEDEALER": return "LIVE DEALER";
                default: return "OTHER GAMES";
            }
        }

        $scope.namesTags = [];
        function getNameTags() {
            return $filter('map')($scope.namesTags, function (t) { return t.text; });
        }

        $scope.searching = function() {
            return $filter('where')($scope.gameCategories || [], { 'searching': true }).length > 0;
        };

        function loadCategories() {
            $scope.gameCategories = [];
            emCasino.getGameCategories().then(function (getCategoriesResult) {
                for (i = 0; i < getCategoriesResult.categories.length; i++) {
                    $scope.gameCategories.push({
                        name: getCategoriesResult.categories[i],
                        selected: true,
                        collapsed: false,
                        searching: false,
                        currentPageIndex: 0,
                        totalGameCount: 0,
                        totalPageCount: 0,
                        games: []
                    });
                }
                $scope.searchGames();
            }, logError);
        };

        function fetchGamesByCategory(category) {
            category.searching = true;
            emCasino.getGames({
                filterByCategory: [category.name],
                filterByName: getNameTags(),
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize: pageSize,
                pageIndex: category.currentPageIndex + 1,
            }).then(function (getCategoryGamesResult) {
                category.searching = false;
                category.currentPageIndex = getCategoryGamesResult.currentPageIndex;
                category.totalGameCount = getCategoryGamesResult.totalGameCount;
                category.totalPageCount = getCategoryGamesResult.totalPageCount;
                Array.prototype.push.apply(category.games, getCategoryGamesResult.games);
            }, function() {
                category.searching = false;
                console.log(error);
            });
        }

        $scope.fetchNextPage = function (category) {
            fetchGamesByCategory(category);
        }

        $scope.searchGames = function () {
            var names = getNameTags();
            for (i = 0; i < $scope.gameCategories.length; i++) {
                var category = $scope.gameCategories[i];
                category.games = [];
                category.collapsed = false;
                category.currentPageIndex = 0;
                category.totalGameCount = 0;
                category.totalPageCount = 0;
                if (category.selected)
                    fetchGamesByCategory(category, names);
            }
        };

        $scope.onCategorySelected = function (category) {
            category.selected = !category.selected;
        };
        $scope.toggleCategoryCollapse = function(category) {
            category.collapsed = !category.collapsed;
        };
        
        $scope.onGameSelected = function (slug) {
            $location.path(constants.routes.game.path.replace(":slug", slug));
        };
        // #endregion
        
        $scope._init('games', function () {
            loadCategories();
            getFeaturedGames();
        });

        function logError(error) {
            console.log(error);
        };
    }
})();
