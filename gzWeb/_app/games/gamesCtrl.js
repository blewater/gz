(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'iso4217', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, iso4217, helpers) {
        $controller('authCtrl', { $scope: $scope });

        // #region Variables
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.spinnerGreenXs = constants.spinners.xs_rel_green;
        $scope.spinnerWhiteXs = constants.spinners.xs_rel_white;
        $scope.spinnerGreenXsAbs = constants.spinners.xs_abs_green;
        var i = 0;
        var pageSize = 6;
        $scope.playForRealMoney = true;
        // #endregion


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
                $scope.carouselGames = $scope.mostPopularGames.slice(0, 5);
                for (i = 0; i < $scope.carouselGames.length; i++) {
                    populateGame($scope.carouselGames[i], i);
                }
            }, logError);
            function populateGame(game, i) {
                game.title = "Title " + (i + 1);
                game.subtitle = "Subtitle subtitle subtitle " + (i + 1);
                game.url = $scope.getGameUrl(game.slug);
            }
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
            //getRecommendedGames();
            //getBiggestWinGames();
            //getLastPlayedGames();
            //getMostPlayedGames();
            //getJackpots();
        };
        // #endregion


        // #region LiveSearch
        $scope.liveSearch = {
            input: "",
            focused: false,
            searching: false,
            selectedIndex: -1,
            pageSize: 10,
            currentPageIndex: 1,
            totalGameCount: 0,
            totalPageCount: 0,
            games: [],
            noResults: ""
        };
        function search(index) {
            $scope.liveSearch.searching = true;
            if ($scope.liveSearch.totalPageCount === 0 || index <= $scope.liveSearch.totalPageCount) {
                emCasino.getGames({
                    filterByPlatform: null,
                    filterByName: [$scope.liveSearch.input],
                    //filterBySlug: [$scope.liveSearch.input],
                    expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Description + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Logo,
                    pageSize: 10,
                    pageIndex: index,
                }).then(function (liveSearchResult) {
                    $scope.liveSearch.searching = false;
                    $scope.liveSearch.showResults = true;
                    $scope.liveSearch.currentPageIndex = liveSearchResult.currentPageIndex;
                    $scope.liveSearch.totalGameCount = liveSearchResult.totalGameCount;
                    $scope.liveSearch.totalPageCount = liveSearchResult.totalPageCount;
                    if (index > 1)
                        Array.prototype.push.apply($scope.liveSearch.games, liveSearchResult.games);
                    else
                        $scope.liveSearch.games = liveSearchResult.games;
                }, function () {
                    $scope.liveSearch.searching = false;
                    console.log(error);
                });
            }
        }
        $scope.onLiveSearchFocused = function () {
            $scope.liveSearch.focused = true;
            $scope.liveSearch.showResults = $scope.liveSearch.games.length > 0;
        };
        $scope.onLiveSearchBlurred = function () {
            $scope.liveSearch.focused = false;
        };
        //$scope.onLiveSearchChange = function () {
        //    if ($scope.liveSearch.input.length > 2) {
        //        $scope.liveSearch.currentPageIndex = 1;
        //        $scope.liveSearch.totalGameCount = 0;
        //        $scope.liveSearch.totalPageCount = 0;
        //        $scope.liveSearch.games = [];
        //        $timeout(function () {
        //            search(1);
        //        }, 400);
        //    }
        //};
        $scope.onLiveSearchChange = function () {
            $scope.liveSearch.currentPageIndex = 1;
            $scope.liveSearch.totalGameCount = 0;
            $scope.liveSearch.totalPageCount = 0;
            $scope.liveSearch.games = [];
            search(1);
        };

        $scope.onLoadMore = function () {
            search($scope.liveSearch.currentPageIndex + 1);
        };
        $scope.selectResult = function (index) {
            for (i = 0; i < $scope.liveSearch.games.length; i++)
                $scope.liveSearch.games[i].selected = i === index;
        };
        $scope.showResults = function () {
            liveSearch.input.length > 2;
        };
        // #endregion


        // #region Filtered Games
        function loadCategories() {
            $scope.gameCategories = [];
            $scope.customCategories = [];
            emCasino.getGameCategories().then(function (getCategoriesResult) {
                for (i = 0; i < getCategoriesResult.categories.length; i++) {
                    $scope.gameCategories.push({
                        name: getCategoriesResult.categories[i],
                        title: getCategoryTitle(getCategoriesResult.categories[i]),
                        selected: false,
                        searching: false,
                        currentPageIndex: 0,
                        totalGameCount: 0,
                        totalPageCount: 0,
                        games: []
                    });
                }
                for (i = 0; i < 3; i++) {
                    var customCategory = {
                        name: getCategoriesResult.categories[i],
                        title: "Custom Category " + (i + 1),
                        currentPageIndex: 0,
                        games: []
                    };
                    $scope.customCategories.push(customCategory);
                    fetchGamesByCategory(customCategory)
                }
                $scope.projectedCategories = $scope.customCategories;
            }, logError);
        };

        function getFriendlyTitle(name) {
            switch (name) {
                case "VIDEOSLOTS": return "Video slots";
                case "JACKPOTGAMES": return "Jackpot";
                case "MINIGAMES": return "Mini games";
                case "CLASSICSLOTS": return "Classic slots";
                case "LOTTERY": return "Lottery";
                case "VIDEOPOKERS": return "Video pokers";
                case "TABLEGAMES": return "Table games";
                case "SCRATCHCARDS": return "Scratch cards";
                case "3DSLOTS": return "3D slots";
                case "LIVEDEALER": return "Live dealer";
                default: return "Other games";
            }
        }

        function getCategoryTitle (name) {
            return getFriendlyTitle(name).replace(/ /g, '\xa0');
        }





        $scope.onCategorySelected = function (index) {
            for (i = 0; i < $scope.gameCategories.length; i++) {
                if (i !== index)
                    $scope.gameCategories[i].selected = false;
            }
            $scope.gameCategories[index].selected = !$scope.gameCategories[index].selected;

            if ($scope.gameCategories[index].selected) {
                $scope.projectedCategories = [$scope.gameCategories[index]];
                if ($scope.gameCategories[index].games.length === 0)
                    fetchGamesByCategory($scope.gameCategories[index], true);
            }
            else {
                $scope.projectedCategories = $scope.customCategories;
            }
        };

        function fetchGamesByCategory(category, recursive) {
            category.searching = true;
            emCasino.getGames({
                filterByCategory: [category.name],
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize: pageSize,
                pageIndex: category.currentPageIndex + 1,
            }).then(function (getCategoryGamesResult) {
                category.searching = false;
                category.currentPageIndex = getCategoryGamesResult.currentPageIndex;
                category.totalGameCount = getCategoryGamesResult.totalGameCount;
                category.totalPageCount = getCategoryGamesResult.totalPageCount;
                //Array.prototype.push.apply(category.games, getCategoryGamesResult.games);

                helpers.array.applyWithDelay(getCategoryGamesResult.games, function (g) {
                    Array.prototype.push.apply(category.games, [g]);
                }, 50);

                if (recursive && category.games.length < category.totalGameCount) {
                    $timeout(function () {
                        fetchGamesByCategory(category, recursive);
                    }, 250);
                }
            }, function() {
                category.searching = false;
                console.log(error);
            });
        }

        $scope.fetchNextPage = function (category) {
            fetchGamesByCategory(category);
        }

        $scope.searchGames = function () {
            for (i = 0; i < $scope.gameCategories.length; i++) {
                var category = $scope.gameCategories[i];
                category.games = [];
                category.collapsed = false;
                category.currentPageIndex = 0;
                category.totalGameCount = 0;
                category.totalPageCount = 0;
                if (category.selected)
                    fetchGamesByCategory(category);
            }
        };

        //$scope.onGameSelected = function (slug) {
        //    $location.path(constants.routes.game.path.replace(":slug", slug));
        //};
        $scope.getGameUrl = function (slug) {
            return constants.routes.game.path.replace(":slug", slug);
        }
        //$scope.getGamesChunks = function (games) {
        //    var chunkSize = $rootScope.xs ? 2 : ($rootScope.sm ? 3 : 6)
        //    var chunks = helpers.array.chunk(games, chunkSize);
        //    return chunks;
        //}
        // #endregion
        

        // #region Init
        $scope._init(function () {
            loadCategories();
            getFeaturedGames();
        });

        function logError(error) {
            console.log(error);
        };
        // #endregion
    }
})();
