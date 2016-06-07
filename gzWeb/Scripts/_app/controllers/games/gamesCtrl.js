(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, helpers) {
        $controller('authCtrl', { $scope: $scope });

        var i = 0;
        $scope.playForRealMoney = true;

        // #region Featured Games
        var featuredGameFields = emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity + emCasino.FIELDS.Logo + emCasino.FIELDS.BackgroundImage;
        
        function getMostPopularGames() {
            $scope.mostPopularGames = [];
            var parameters = {
                expectedFields: featuredGameFields,
                expectedFormat: 'array',
                pageIndex: 1,
                pageSize: 5,
                sortFields: [{ field: emCasino.FIELDS.Popularity, order: 'DESC' }]
            };
            emCasino.getGames(parameters).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j];
                    game.rank = j + 1;
                    $scope.mostPopularGames.push(game);
                }
            }, logError);
        }

        function getRecommendedGames() {
            $scope.recommendedGames = [];
            var parameters = {
                recommendedType: "user",
                expectedGameFields: featuredGameFields
            };
            emCasino.getRecommendedGames(parameters).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j].game;
                    game.rank = j + 1;
                    $scope.recommendedGames.push(game);
                }
            }, logError);
        }

        function getBiggestWinGames() {
            $scope.biggestWinGames = [];
            emCasino.getBiggestWinGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j].game;
                    game.rank = j + 1;
                    $scope.biggestWinGames.push(game);
                }
            }, logError);
        }

        function getLastPlayedGames() {
            $scope.lastPlayedGames = [];
            emCasino.getLastPlayedGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j].game;
                    game.rank = j + 1;
                    $scope.lastPlayedGames.push(game);
                }
            }, logError);
        }

        function getMostPlayedGames() {
            $scope.mostPlayedGames = [];
            emCasino.getMostPlayedGames(featuredGameFields).then(function (result) {
                for (var j = 0; j < result.games.length; j++) {
                    var game = result.games[j].game;
                    game.rank = j + 1;
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
                    var game = result.jackpots[j].game;
                    game.rank = j + 1;
                    $scope.jackpots.push(game);
                }
            }, logError);
        }

        function getFeaturedGames() {
            getMostPopularGames();
            getRecommendedGames();
            getBiggestWinGames();
            getLastPlayedGames();
            getMostPlayedGames();
            getJackpots();
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


        var allCategoryName = "All";
        var allCategory = { name: allCategoryName, selected: true };
        var categories = [];

        function loadGameCategories() {
            $scope.gameCategories = [];
            emCasino.getGameCategories().then(function (getCategoriesResult) {
                for (i = 0; i < getCategoriesResult.categories.length; i++)
                    loadCategoryGames(getCategoriesResult.categories[i]);
            }, logError);
        };

        function loadCategoryGames(categoryName) {
            emCasino.getGames({
                filterByCategory: [categoryName],
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize: 4
            }).then(function (getCategoryGamesResult) {
                $scope.gameCategories.push({
                    name: categoryName,
                    selected: true,
                    collapsed: false,
                    games: getCategoryGamesResult.games
                });
            }, logError);
        }
        //function anySelected() {
        //    helpers.array.any(categories, function(c) { return c.selected; });
        //}
        //function allSelected() {
        //    helpers.array.all(categories, function (c) { return c.selected; });
        //}

        //$scope.isCategory = function () {
        //    return function (c) {
        //        return c.selected || allCategory.selected;
        //    };
        //};

        $scope.onCategorySelected = function (category) {
            category.selected = !category.selected;
        };
        $scope.toggleCategoryCollapsed = function(category) {
            category.collapsed = !category.collapsed;
        };


        $scope.onGameSelected = function (slug) {
            $location.path(constants.routes.game.path.replace(":slug", slug));
        };


        function getGames() {
            emCasino.getGames({
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize:4
            }).then(function(result) {
                $scope.games = result.games;
            }, logError);
        };
        
        $scope._init('games', function () {
            loadGameCategories();
            getFeaturedGames();
            getRecommendedGames();
        });

        function logError(error) {
            console.log(error);
        };
    }
})();
