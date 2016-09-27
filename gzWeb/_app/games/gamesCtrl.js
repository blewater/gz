(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'iso4217', 'helpers', '$log', 'api', '$q', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, iso4217, helpers, $log, api, $q) {
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
                if (names.length > 0 || vendors.length > 0 || $scope.customCategories.length === 0) {
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
        function searchGames(category, names, vendors, callback) {
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
                            searchGames(category, names, vendors, callback);
                        }, 250);
                    }
                    else if (angular.isDefined(callback))
                        callback();
                });
            }, function (error) {
                $scope.fetching = false;
                $log.error(error);
            });
        }

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
                case "OTHERGAMES": return "Other games";
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
        };

        $scope.onSearchByVendor = function (selectedVendor) {
            $location.search('vendor', selectedVendor ? selectedVendor.toLowerCase() : null);
            $timeout(function () {
                search();
            }, 0);
        };

        $scope.thereAreNoResults = function () {
            return !$scope.fetching && ($scope.projectedCategories.length === 0 || helpers.array.all($scope.projectedCategories, function (c) {
                return c.games.length === 0;
            }));
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
        function getCarouselUrl(carouselEntry) {
            switch (carouselEntry.ActionType) {
                case constants.carouselActionTypes.page:
                    return '/promotions/' + carouselEntry.ActionUrl;
                case constants.carouselActionTypes.game:
                    return '/casino/' + carouselEntry.ActionUrl;
                case constants.carouselActionTypes.url:
                default:
                    return carouselEntry.ActionUrl.substring(0, 4) === 'http'
                           ? carouselEntry.ActionUrl
                           : $location.protocol() + "://" + $location.host() + ":" + $location.port() + carouselEntry.ActionUrl;
            }
        }
        function constructCarouselSlide(carouselEntry) {
            if (carouselEntry.BackgroundImageUrl) {
                return {
                    title: carouselEntry.Title,
                    subtitle: carouselEntry.SubTitle,
                    action: carouselEntry.ActionText,
                    url: getCarouselUrl(carouselEntry),
                    bg: carouselEntry.BackgroundImageUrl
                };
            }
            else if (carouselEntry.ActionType === constants.carouselActionTypes.game) {
                var deferred = $q.defer();
                emCasino.getGames({
                    filterBySlug: [carouselEntry.ActionUrl],
                    filterByPlatform: null,
                    expectedFields: emCasino.FIELDS.BackgroundImage,
                    pageSize: 1,
                    pageIndex: 1
                }).then(function (gameResult) {
                    deferred.resolve({
                        title: carouselEntry.Title,
                        subtitle: carouselEntry.SubTitle,
                        action: carouselEntry.ActionText,
                        url: getCarouselUrl(carouselEntry),
                        bg: gameResult.games[0].backgroundImage
                    });
                }, function (error) {
                    $log.error(error);
                });
                return deferred.promise;
            }
        }
        function loadCarouselSlides() {
            api.call(function () {
                return api.getCarousel();
            }, function (response) {
                //$scope.carouselSlides = $filter('map')(
                //    response.Result,
                //    function (x) {
                //        return {
                //            title: x.Title,
                //            subtitle: x.SubTitle,
                //            action: x.ActionText,
                //            url: x.ActionUrl.substring(0, 4) === 'http'
                //                 ? x.ActionUrl
                //                 : $location.protocol() + "://" + $location.host() + ":" + $location.port() + x.ActionUrl,
                //            bg: x.BackgroundImageUrl
                //        };
                //    });


                //for (i = 0; i < response.Result.length; i++) {
                //    if (response.Result[i].BackgroundImageUrl) {
                //        $scope.carouselSlides.
                //    }
                //    x.ActionType === constants.carouselActionTypes.game)
                //}


                var mapPromises = $filter('map')(
                    response.Result,
                    function (x) {
                        var deferred = $q.defer();
                        deferred.resolve(constructCarouselSlide(x));
                        return deferred.promise;
                    });

                $q.all(mapPromises).then(function (x) {
                    $scope.carouselSlides = x;
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
        function loadCategories() {
            var deferred = $q.defer();
            $scope.gameCategories = [];
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
                deferred.resolve(true);
            }, logError);
            return deferred.promise;
        };
        function loadCustomCategories() {
            var deferred = $q.defer();
            api.call(function () {
                return api.getCustomCategories();
            }, function (response) {
                var categoriesEntries = response.Result;
                $scope.customCategories = [];
                for (var j = 0; j < categoriesEntries.length; j++) {
                    var customCategory = {
                        name: categoriesEntries[j].Code,
                        title: categoriesEntries[j].Title,
                        currentPageIndex: 0,
                        games: [],
                        paging: $scope.pagingTypes.row,
                        sorting: alphaAsc
                    };
                    $scope.customCategories.push(customCategory);

                    $scope.fetching = true;
                    emCasino.getGames({
                        filterBySlug: categoriesEntries[j].GameSlugs,
                        filterByPlatform: null,
                        expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity,
                        pageSize: customCategory.paging.size,
                        pageIndex: customCategory.currentPageIndex + 1
                    }).then(function (getCategoryGamesResult) {
                        $scope.fetching = false;

                        customCategory.currentPageIndex = getCategoryGamesResult.currentPageIndex;
                        customCategory.totalGameCount = getCategoryGamesResult.totalGameCount;
                        customCategory.totalPageCount = getCategoryGamesResult.totalPageCount;
                        //Array.prototype.push.apply(category.games, getCategoryGamesResult.games);

                        helpers.array.applyWithDelay(getCategoryGamesResult.games, function (g) {
                            Array.prototype.push.apply(customCategory.games, [g]);
                        }, 50, function () {
                            deferred.resolve(true);
                        });
                    }, function (error) {
                        $scope.fetching = false;
                        $log.error(error);
                    });
                }
            });
            return deferred.promise;
        }
        $scope._init(function () {
            readParams();
            loadCarouselSlides();
            loadGameVendors();

            $q.all([loadCategories(), loadCustomCategories()]).then(function () {
                search();
            });

            //loadCategories();
            //loadCustomCategories();
            //getFeaturedGames();
        });

        function logError(error) {
            $log.error(error);
        };
        // #endregion
    }
})();
