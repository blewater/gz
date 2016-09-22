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
        $scope.pagingTypes = {
            row: { key: 'row', size: pageSize, recursive: false },
            less: { key: 'less', size: 3 * pageSize - 1, recursive: false },
            all: { key: 'all', size: 3 * pageSize - 1, recursive: true },
        }
        //$scope.sortingByTypes = ['Alphabetically', 'Popularity'];
        //$scope.sortingOrderTypes = ['Ascending', 'Descending'];
        //$scope.sortingTypes = ['A-Z, Asc', 'A-Z, Desc', 'Popularity, Asc', 'Popularity Desc'];
        var alphaAsc = { key: 'alphaAsc', display: 'Name: A -> Z', by: 'name', order: false };
        var alphaDesc = { key: 'alphaDesc', display: 'Name: Z -> A', by: 'name', order: true };
        var popularityAsc = { key: 'popularityAsc', display: 'Popularity: Low -> High', by: 'popularity', order: false };
        var popularityDesc = { key: 'popularityDesc', display: 'Popularity: High -> Low', by: 'popularity', order: true };

        $scope.sortingTypes = [alphaAsc, alphaDesc, popularityDesc, popularityAsc];
        $scope.sorting = alphaAsc;

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
            return constants.routes.game.path.replace(":slug", slug) + $location.url().substring($location.path().length);
        }

        function search() {
            var names = $scope.searchByNameTerm.length > 0 ? [$scope.searchByNameTerm] : [];
            var vendors = $scope.searchByVendorTerm ? [$scope.searchByVendorTerm] : [];
            if ($scope.selectedCategory) {
                $scope.projectedCategories = [$scope.selectedCategory];
                if ($scope.selectedCategory.games.length === 0 || names.length > 0 || vendors.length > 0) {
                    $scope.selectedCategory.currentPageIndex = 0;
                    $scope.selectedCategory.games = [];
                    searchGames($scope.selectedCategory, names, vendors);
                }
                else if ($scope.selectedCategory.paging.key === $scope.pagingTypes.all.key) {
                    searchGames($scope.selectedCategory, names, vendors);
                }
            }
            else {
                if (names.length > 0 || vendors.length > 0) {
                    $scope.projectedCategories = $scope.gameCategories;
                    for (i = 0; i < $scope.gameCategories.length; i++) {
                        $scope.gameCategories[i].currentPageIndex = 0;
                        $scope.gameCategories[i].games = [];
                        $scope.gameCategories[i].pagingMode = $scope.pagingTypes.less;
                        searchGames($scope.gameCategories[i], names, vendors);
                    }
                }
                else {
                    $scope.projectedCategories = $scope.customCategories;
                }
            }
        }
        function searchGames(category, names, vendors) {
            $scope.fetching = true;

            emCasino.getGames({
                filterByCategory: [category.name],
                filterByName: names || [],
                filterByVendor: vendors || [],
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity,
                pageSize: category.paging.size,
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
                }, 50, function () {
                    if (category.paging.recursive && category.games.length < category.totalGameCount) {
                        $timeout(function () {
                            searchGames(category, names, vendors);
                        }, 250);
                    }
                });
            }, function (error) {
                $scope.fetching = false;
                $log.error(error);
            });
        }








        function fetchGames(category, name, vendor, recursive) {
            $scope.fetching = true;
            emCasino.getGames({
                filterByCategory: [category.name],
                filterByName: name ? [name] : [],
                filterByVendor: vendor ? [vendor] : [],
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,// + emCasino.FIELDS.Categories,
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
                    games: [],
                    paging: $scope.pagingTypes.row,
                    sorting: alphaAsc
                };
                $scope.customCategories.push(customCategory);
                //fetchGames(customCategory);
                searchGames(customCategory);
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
                        games: [],
                        paging: $scope.pagingTypes.less,
                        sorting: alphaAsc
                    };
                    $scope.gameCategories.push(category);
                    if (isSelected)
                        $scope.selectedCategory = category;
                }

                loadCustomCategories();

                search();
                //if ($scope.selectedCategory) {
                //    $scope.projectedCategories = [$scope.selectedCategory];
                //    $scope.selectedCategory.currentPageIndex = 0;
                //    $scope.selectedCategory.games = [];
                //    fetchGames($scope.selectedCategory, $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                //}
                //else if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
                //    $scope.projectedCategories = $scope.gameCategories;
                //    for (i = 0; i < $scope.gameCategories.length; i++) {
                //        $scope.gameCategories[i].currentPageIndex = 0;
                //        $scope.gameCategories[i].games = [];
                //        fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
                //    }
                //}
                //else {
                //    $scope.projectedCategories = $scope.customCategories;
                //}
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
                default: return name;
            }
        }

        function getCategoryTitle (name) {
            return getFriendlyTitle(name).replace(/ /g, '\xa0');
        }

        $scope.onSortingChanged = function (sorting, category) {
            category.sorting = sorting;
        };

        $scope.onCategorySelected = function (index) {
            for (i = 0; i < $scope.gameCategories.length; i++)
                if (i !== index)
                    $scope.gameCategories[i].selected = false;
            $scope.gameCategories[index].selected = !$scope.gameCategories[index].selected;

            $location.search('category', $scope.gameCategories[index].selected ? $scope.gameCategories[index].name.toLowerCase() : null);

            $scope.selectedCategory = $scope.gameCategories[index].selected ? $scope.gameCategories[index] : undefined;
            search();

            //if ($scope.gameCategories[index].selected) {
            //    $scope.selectedCategory = $scope.gameCategories[index];
            //    $scope.projectedCategories = [$scope.gameCategories[index]];
            //    if ($scope.gameCategories[index].games.length === 0 || $scope.searchByNameTerm.length > 0) {
            //        $scope.gameCategories[index].currentPageIndex = 0;
            //        $scope.gameCategories[index].games = [];
            //        fetchGames($scope.gameCategories[index], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
            //    }
            //}
            //else if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
            //    $scope.selectedCategory = undefined;
            //    $scope.projectedCategories = $scope.gameCategories;
            //    for (i = 0; i < $scope.gameCategories.length; i++) {
            //        $scope.gameCategories[i].currentPageIndex = 0;
            //        $scope.gameCategories[i].games = [];
            //        fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
            //    }
            //}
            //else {
            //    $scope.selectedCategory = undefined;
            //    $scope.projectedCategories = $scope.customCategories;
            //}
        };

        var searchByNameTimeoutPromise = undefined;
        var searchByNameTimeoutDelay = 250;
        $scope.onSearchByName = function () {
            if (angular.isDefined(searchByNameTimeoutPromise))
                $timeout.cancel(searchByNameTimeoutPromise);

            searchByNameTimeoutPromise = $timeout(function () {
                $location.search('name', $scope.searchByNameTerm.length > 0 ? $scope.searchByNameTerm : null);
                search();
            }, searchByNameTimeoutDelay);
            //if ($scope.selectedCategory) {
            //    $scope.selectedCategory.currentPageIndex = 0;
            //    $scope.selectedCategory.games = [];
            //    $scope.projectedCategories = [$scope.selectedCategory];
            //    fetchGames($scope.selectedCategory, $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
            //}
            //else {
            //    if ($scope.searchByNameTerm.length > 0 || $scope.searchByVendorTerm) {
            //        $scope.projectedCategories = $scope.gameCategories;
            //        for (i = 0; i < $scope.gameCategories.length; i++) {
            //            $scope.gameCategories[i].currentPageIndex = 0;
            //            $scope.gameCategories[i].games = [];
            //            fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, $scope.searchByVendorTerm, true);
            //        }
            //    }
            //    else {
            //        $scope.projectedCategories = $scope.customCategories;
            //    }
            //}
        };

        $scope.onSearchByVendor = function (selectedVendor) {
            $location.search('vendor', selectedVendor ? selectedVendor.toLowerCase() : null);
            $timeout(function () {
                search();
            }, 0);
            //if ($scope.selectedCategory) {
            //    $scope.selectedCategory.currentPageIndex = 0;
            //    $scope.selectedCategory.games = [];
            //    $scope.projectedCategories = [$scope.selectedCategory];
            //    fetchGames($scope.selectedCategory, $scope.searchByNameTerm, selectedVendor, true);
            //}
            //else {
            //    if ($scope.searchByNameTerm.length > 0 || selectedVendor) {
            //        $scope.projectedCategories = $scope.gameCategories;
            //        for (i = 0; i < $scope.gameCategories.length; i++) {
            //            $scope.gameCategories[i].currentPageIndex = 0;
            //            $scope.gameCategories[i].games = [];
            //            fetchGames($scope.gameCategories[i], $scope.searchByNameTerm, selectedVendor, true);
            //        }
            //    }
            //    else {
            //        $scope.projectedCategories = $scope.customCategories;
            //    }
            //}
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

        $scope.showAll = function (category) {
            category.paging = $scope.pagingTypes.all;
            search();
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
                            url: x.ActionUrl.substring(0, 4) === 'http'
                                 ? x.ActionUrl
                                 : $location.protocol() + "://" + $location.host() + ":" + $location.port() + x.ActionUrl,
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
