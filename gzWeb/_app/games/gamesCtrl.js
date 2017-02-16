(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$timeout', '$interval', '$filter', 'emCasino', 'constants', 'iso4217', 'helpers', '$log', 'api', '$q', '$rootScope', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $timeout, $interval, $filter, emCasino, constants, iso4217, helpers, $log, api, $q, $rootScope) {
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
        var alphaAsc = { key: 'alphaAsc', display: 'Name: A -> Z', by: 'name', order: false, emField: emCasino.FIELDS.Name, emOrder: 'ASC' };
        var alphaDesc = { key: 'alphaDesc', display: 'Name: Z -> A', by: 'name', order: true, emField: emCasino.FIELDS.Name, emOrder: 'DESC' };
        var popularityAsc = { key: 'popularityAsc', display: 'Popularity: Low -> High', by: 'popularity', order: false, emField: emCasino.FIELDS.Popularity, emOrder: 'ASC' };
        var popularityDesc = { key: 'popularityDesc', display: 'Popularity: High -> Low', by: 'popularity', order: true, emField: emCasino.FIELDS.Popularity, emOrder: 'DESC' };

        $scope.sortingTypes = [alphaAsc, alphaDesc, popularityDesc, popularityAsc];
        $scope.sorting = alphaAsc;

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
        $scope.getGameUrl = function (slug, playForFun) {
            return constants.routes.game.path.replace(":slug", slug) + '?funMode=' + (playForFun || false) + '&' + $location.url().substring($location.path().length + 1);
        }

        function search() {
            var names = $scope.searchByNameTerm.length > 0 ? [$scope.searchByNameTerm] : [];
            var vendors = $scope.searchByVendorTerm ? [$scope.searchByVendorTerm] : [];
            if ($scope.selectedCategory) {
                $scope.projectedCategories = [$scope.selectedCategory];
                if ($scope.selectedCategory.games.length === 0 || names.length > 0 || vendors.length > 0) {
                    $scope.selectedCategory.currentPageIndex = 0;
                    $scope.selectedCategory.games = [];
                    searchGamesByCategory($scope.selectedCategory, names, vendors);
                }
                else if ($scope.selectedCategory.paging.key === $scope.pagingTypes.all.key) {
                    searchGamesByCategory($scope.selectedCategory, names, vendors);
                }
            }
            else {
                if (names.length > 0 || vendors.length > 0 || ($scope.customCategories && $scope.customCategories.length === 0)) {
                    $scope.projectedCategories = $scope.gameCategories;
                    for (i = 0; i < $scope.gameCategories.length; i++) {
                        $scope.gameCategories[i].currentPageIndex = 0;
                        $scope.gameCategories[i].games = [];
                        $scope.gameCategories[i].pagingMode = $scope.pagingTypes.less;
                        searchGamesByCategory($scope.gameCategories[i], names, vendors);
                    }
                }
                else {
                    $scope.projectedCategories = $scope.customCategories;
                }
            }
            window.appInsights.trackEvent("GAMES SEARCH", {
                name: $scope.searchByNameTerm || 'ALL',
                vendors: $scope.searchByVendorTerm || 'ALL',
                category: $scope.selectedCategory ? $scope.selectedCategory.name : 'ALL'
            });
        }
        function searchGamesByCategory(category, names, vendors, callback) {
            emCasino.getGames({
                filterByCategory: category.all ? [] : [category.name],
                filterByName: names || [],
                filterByVendor: vendors || [],
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity,
                pageSize: category.paging.size,
                pageIndex: category.currentPageIndex + 1,
                sortFields: [
                    { field : category.sorting.emField, order : category.sorting.emOrder }
                ]
            }).then(function (getCategoryGamesResult) {
                category.currentPageIndex = getCategoryGamesResult.currentPageIndex;
                category.totalGameCount = getCategoryGamesResult.totalGameCount;
                category.totalPageCount = getCategoryGamesResult.totalPageCount;
                //Array.prototype.push.apply(category.games, getCategoryGamesResult.games);

                helpers.array.applyWithDelay(getCategoryGamesResult.games, function (g) {
                    Array.prototype.push.apply(category.games, [g]);
                }, 0, function () {
                    $scope.inited = true;
                    if (category.paging.recursive && category.games.length < category.totalGameCount) {
                        $timeout(function () {
                            searchGamesByCategory(category, names, vendors, callback);
                        }, 0);
                    }
                    else if (angular.isDefined(callback))
                        callback();
                });
            }, function (error) {
                $log.error(error);
            });
        }
        function searchGamesBySlugs(category, slugs, callback) {
            emCasino.getGames({
                filterBySlug: slugs,
                filterByPlatform: null,
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Popularity,
                pageSize: category.paging.size,
                pageIndex: category.currentPageIndex + 1,
                sortFields: [
                    { field: category.sorting.emField, order: category.sorting.emOrder }
                ]
            }).then(function (result) {
                category.currentPageIndex = result.currentPageIndex;
                category.totalGameCount = result.totalGameCount;
                category.totalPageCount = result.totalPageCount;

                helpers.array.applyWithDelay(result.games, function (g) {
                    Array.prototype.push.apply(category.games, [g]);
                }, 50, callback);
            }, function (error) {
                $log.error(error);
            });
        }

        var categories = [];
        categories["VIDEOSLOTS"] = { name: "VIDEOSLOTS", title: 'Video slots', include: true, all: false };
        categories["JACKPOTGAMES"] = { name: "JACKPOTGAMES", title: 'Jackpot games', include: true, all: false };
        categories["MINIGAMES"] = { name: "MINIGAMES", title: 'Mini games', include: false, all: false };
        categories["CLASSICSLOTS"] = { name: "CLASSICSLOTS", title: 'Classic slots', include: true, all: false };
        categories["LOTTERY"] = { name: "LOTTERY", title: 'Lottery', include: false, all: false };
        categories["VIDEOPOKERS"] = { name: "VIDEOPOKERS", title: 'Video pokers', include: true, all: false };
        categories["TABLEGAMES"] = { name: "TABLEGAMES", title: 'Table games', include: true, all: false };
        categories["SCRATCHCARDS"] = { name: "SCRATCHCARDS", title: 'Scratch tables', include: true, all: false };
        categories["3DSLOTS"] = { name: "3DSLOTS", title: '3D slots', include: true, all: false };
        categories["LIVEDEALER"] = { name: "LIVEDEALER", title: 'Live dealer', include: true, all: false };
        categories["OTHERGAMES"] = { name: "OTHERGAMES", title: 'Other games', include: true, all: false };
        categories["ALLGAMES"] = { name: "ALLGAMES", title: 'All games', include: true, all: true };
        //console.log(categories);
            
        //function getFriendlyTitle(name) {
        //    switch (name) {
        //        case "VIDEOSLOTS": return "Video slots";
        //        case "JACKPOTGAMES": return "Jackpot";
        //        case "MINIGAMES": return "Mini games";
        //        case "CLASSICSLOTS": return "Classic slots";
        //        case "LOTTERY": return "Lottery";
        //        case "VIDEOPOKERS": return "Video pokers";
        //        case "TABLEGAMES": return "Table games";
        //        case "SCRATCHCARDS": return "Scratch cards";
        //        case "3DSLOTS": return "3D slots";
        //        case "LIVEDEALER": return "Live dealer";
        //        case "OTHERGAMES": return "Other games";
        //        default: return name;
        //    }
        //}

        //function getCategoryTitle (name) {
        //    return getFriendlyTitle(name).replace(/ /g, '\xa0');
        //}

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
            return $scope.inited && ($scope.projectedCategories.length === 0 || helpers.array.all($scope.projectedCategories, function (c) {
                return c.games.length === 0;
            }));
        };

        $scope.showAll = function (category) {
            category.paging = $scope.pagingTypes.all;

            var names = $scope.searchByNameTerm.length > 0 ? [$scope.searchByNameTerm] : [];
            var vendors = $scope.searchByVendorTerm ? [$scope.searchByVendorTerm] : [];
            searchGamesByCategory(category, names, vendors);
            //search();
        };
        $scope.onSortingChanged = function (sorting, category) {
            category.currentPageIndex = 0;
            category.games = [];
            if (category.isCustom) {
                searchGamesBySlugs(category, category.gameSlugs);
            }
            else {
                var names = $scope.searchByNameTerm.length > 0 ? [$scope.searchByNameTerm] : [];
                var vendors = $scope.searchByVendorTerm ? [$scope.searchByVendorTerm] : [];
                searchGamesByCategory(category, names, vendors);
            }
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
                    return '/casino/' + carouselEntry.ActionUrl + '?fun=false';
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
                        bg: gameResult.games[0] ? gameResult.games[0].backgroundImage : '../../Content/Images/casino-default.jpg'
                    });
                }, function (error) {
                    $log.error(error);
                });
                return deferred.promise;
            }
        }
        function loadCarouselSlides() {
            api.call(function () {
                return api.getCarousel($rootScope.mobile);
            }, function (response) {
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
        function addCategory(category) {
            if (category.include) {
                var isSelected = selectedCategoryName === category.name.toLowerCase();
                var gameCategory = {
                    name: category.name,
                    title: category.title.replace(/ /g, '\xa0'),
                    selected: isSelected,
                    currentPageIndex: 0,
                    totalGameCount: 0,
                    totalPageCount: 0,
                    games: [],
                    paging: $scope.pagingTypes.less,
                    sorting: alphaAsc,
                    isCustom: false,
                    all: category.all
                };
                $scope.gameCategories.push(gameCategory);
                if (isSelected)
                    $scope.selectedCategory = gameCategory;
            }
        };
        function loadCategories() {
            var deferred = $q.defer();
            $scope.gameCategories = [];
            emCasino.getGameCategories().then(function (getCategoriesResult) {
                for (i = 0; i < getCategoriesResult.categories.length; i++)
                    addCategory(categories[getCategoriesResult.categories[i]]);
                addCategory(categories.ALLGAMES);
                deferred.resolve(true);
            }, logError);
            return deferred.promise;
        };

        function loadCustomCategory(categoriesEntry) {
            var deferred = $q.defer();
            var customCategory = {
                name: categoriesEntry.Code,
                title: categoriesEntry.Title,
                currentPageIndex: 0,
                games: [],
                paging: $scope.pagingTypes.row,
                sorting: alphaAsc,
                isCustom: true,
                gameSlugs: categoriesEntry.GameSlugs
            };

            searchGamesBySlugs(customCategory, customCategory.gameSlugs, function () {
                deferred.resolve(customCategory);
            });

            return deferred.promise;
        }
        function loadCustomCategories() {
            var deferred = $q.defer();
            api.call(function () {
                return api.getCustomCategories($rootScope.mobile);
            }, function (response) {
                $q.all($filter('map')(response.Result, loadCustomCategory)).then(function (customCategories) {
                    $scope.customCategories = customCategories;
                    deferred.resolve(true);
                });
            });
            return deferred.promise;
        }

        $scope.inited = false;

        $scope._init(function () {
            readParams();
            loadCarouselSlides();
            loadGameVendors();

            $q.all([loadCategories(), loadCustomCategories()]).then(function () {
                search();
            });
        });

        function logError(error) {
            $log.error(error);
        };
        // #endregion
    }
})();
