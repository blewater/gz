(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'iso4217', 'helpers', '$log', 'api', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, iso4217, helpers, $log, api) {
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
        var selectedCategoryName = "", selectedVendorName = "";
        $scope.selectedCategory = undefined;
        $scope.searchByNameTerm = "";
        $scope.searchByVendorTerm = "";
        $scope.projectedCategories = [];
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
            //getMostPopularGames();
            //getRecommendedGames();
            //getBiggestWinGames();
            //getLastPlayedGames();
            //getMostPlayedGames();
            //getJackpots();
        };
        // #endregion


        // #region Filtered Games
        $scope.getGameUrl = function (slug) {
            return constants.routes.game.path.replace(":slug", slug);
        }

        function search(categories, name, vendor, sort, pages) {
            // TODO
        }

        function fetchGames(category, name, vendor, recursive) {
            $scope.fetching = true;
            emCasino.getGames({
                filterByCategory: [category.name],
                filterByName: name ? [name] : [],
                filterByVendor: vendor ? [vendor] : [],
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize: pageSize,
                pageIndex: category.currentPageIndex + 1,
                //sortFields: [
                //    { field : sortField, order : sortOrder }
                //]
            }).then(function (getCategoryGamesResult) {
                $scope.fetching = false;
                category.currentPageIndex = getCategoryGamesResult.currentPageIndex;
                category.totalGameCount = getCategoryGamesResult.totalGameCount;
                category.totalPageCount = getCategoryGamesResult.totalPageCount;
                //Array.prototype.push.apply(category.games, getCategoryGamesResult.games);

                helpers.array.applyWithDelay(getCategoryGamesResult.games, function (g) {
                    Array.prototype.push.apply(category.games, [g]);
                }, 50);

                if (recursive && category.games.length < category.totalGameCount) {
                    $timeout(function () {
                        fetchGames(category, name, vendor, recursive);
                    }, 250);
                }
            }, function (error) {
                $scope.fetching = false;
                $log.error(error);
            });
        }

        function loadCustomCategories() {
            for (i = 0; i < 3; i++) {
                var customCategory = {
                    name: $scope.gameCategories[i].name,
                    title: "Custom Category " + (i + 1),
                    currentPageIndex: 0,
                    games: []
                };
                $scope.customCategories.push(customCategory);
                fetchGames(customCategory);
            }
        }

        function loadCategories() {
            $scope.gameCategories = [];
            $scope.customCategories = [];
            emCasino.getGameCategories().then(function (getCategoriesResult) {
                for (i = 0; i < getCategoriesResult.categories.length; i++) {
                    var isSelected = selectedCategoryName === getCategoriesResult.categories[i].toLowerCase();
                    var category = {
                        name: getCategoriesResult.categories[i],
                        title: getCategoryTitle(getCategoriesResult.categories[i]),
                        selected: isSelected,
                        currentPageIndex: 0,
                        totalGameCount: 0,
                        totalPageCount: 0,
                        games: []
                    };
                    $scope.gameCategories.push(category);
                    if (isSelected)
                        $scope.selectedCategory = category;
                }

                loadCustomCategories();

                if ($scope.selectedCategory) {
                    $scope.projectedCategories = [$scope.selectedCategory];
                    $scope.selectedCategory.currentPageIndex = 0;
                    $scope.selectedCategory.games = [];
                    fetchGames($scope.selectedCategory, $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                }
                else if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
                    $scope.projectedCategories = $scope.gameCategories;
                    for (i = 0; i < $scope.gameCategories.length; i++) {
                        $scope.gameCategories[i].currentPageIndex = 0;
                        $scope.gameCategories[i].games = [];
                        fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                    }
                }
                else {
                    $scope.projectedCategories = $scope.customCategories;
                }
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
            for (i = 0; i < $scope.gameCategories.length; i++)
                if (i !== index)
                    $scope.gameCategories[i].selected = false;
            $scope.gameCategories[index].selected = !$scope.gameCategories[index].selected;

            $location.search('category', $scope.gameCategories[index].selected ? $scope.gameCategories[index].name.toLowerCase() : null);
            if ($scope.gameCategories[index].selected) {
                $scope.selectedCategory = $scope.gameCategories[index];
                $scope.projectedCategories = [$scope.gameCategories[index]];
                if ($scope.gameCategories[index].games.length === 0 || $scope.searchByNameTerm.length > 0) {
                    $scope.gameCategories[index].currentPageIndex = 0;
                    $scope.gameCategories[index].games = [];
                    fetchGames($scope.gameCategories[index], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                }
            }
            else if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
                $scope.selectedCategory = undefined;
                $scope.projectedCategories = $scope.gameCategories;
                for (i = 0; i < $scope.gameCategories.length; i++) {
                    $scope.gameCategories[i].currentPageIndex = 0;
                    $scope.gameCategories[i].games = [];
                    fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                }
            }
            else {
                $scope.selectedCategory = undefined;
                $scope.projectedCategories = $scope.customCategories;
            }
        };

        $scope.onSearchByName = function () {
            $location.search('name', $scope.searchByNameTerm.length > 0 ? $scope.searchByNameTerm : null);
            if ($scope.selectedCategory) {
                $scope.selectedCategory.currentPageIndex = 0;
                $scope.selectedCategory.games = [];
                $scope.projectedCategories = [$scope.selectedCategory];
                fetchGames($scope.selectedCategory, $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
            }
            else {
                if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
                    $scope.projectedCategories = $scope.gameCategories;
                    for (i = 0; i < $scope.gameCategories.length; i++) {
                        $scope.gameCategories[i].currentPageIndex = 0;
                        $scope.gameCategories[i].games = [];
                        fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                    }
                }
                else {
                    $scope.projectedCategories = $scope.customCategories;
                }
            }
        };

        $scope.onSearchByVendor = function (selectedVendor) {
            $location.search('vendor', selectedVendor ? selectedVendor.toLowerCase() : null);
            if ($scope.selectedCategory) {
                $scope.selectedCategory.currentPageIndex = 0;
                $scope.selectedCategory.games = [];
                $scope.projectedCategories = [$scope.selectedCategory];
                fetchGames($scope.selectedCategory, $scope.searchByNameTerm, selectedVendor, true);
            }
            else {
                if ($scope.searchByNameTerm.length > 0 || selectedVendor) {
                    $scope.projectedCategories = $scope.gameCategories;
                    for (i = 0; i < $scope.gameCategories.length; i++) {
                        $scope.gameCategories[i].currentPageIndex = 0;
                        $scope.gameCategories[i].games = [];
                        fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, selectedVendor, true);
                    }
                }
                else {
                    $scope.projectedCategories = $scope.customCategories;
                }
            }
        };

        $scope.thereAreNoResults = function () {
            return !$scope.fetching && ($scope.projectedCategories.length === 0 || helpers.array.all($scope.projectedCategories, function (c) {
                return c.games.length === 0;
            }));
        };

        function getSelectedCategory() {
            for (i = 0; i < $scope.gameCategories.length; i++)
                if ($scope.gameCategories[i].selected === true)
                    return $scope.gameCategories[i];
            return undefined;
        };
        // #endregion
        

        // #region Init
        function readParams() {
            var params = $location.search();
            selectedCategoryName = params.category || "";
            selectedVendorName = params.vendor || "";
            $scope.searchByNameTerm = params.name || "";
        };
        function loadCarouselSlides() {
            api.call(function () {
                return api.getCarousel();
            }, function (response) {
                $scope.carouselSlides = $filter('map')(
                    response.Result,
                    function (x) {
                        return {
                            title: x.Title,
                            subtitle: x.SubTitle,
                            action: x.ActionText,
                            url: x.ActionUrl,
                            bg: x.BackgroundImageUrl
                        };
                    });
            });
        };
        function loadGameVendors() {
            $scope.loadingVendors = true;
            emCasino.getGameVendors().then(function (getGameVendorsResult) {
                $scope.loadingVendors = false;
                if (selectedVendorName.length > 0) {
                    for (i = 0; i < getGameVendorsResult.vendors.length; i++) {
                        if (selectedVendorName === getGameVendorsResult.vendors[i].toLowerCase()) {
                            $scope.searchByVendorTerm = getGameVendorsResult.vendors[i];
                            break;
                        }
                    }
                }
                $scope.vendors = getGameVendorsResult.vendors;
            }, logError);
        };
        $scope._init(function () {
            readParams();
            loadCarouselSlides();
            loadGameVendors();
            loadCategories();
            //getFeaturedGames();
        });

        function logError(error) {
            $log.error(error);
        };
        // #endregion
    }
})();
