(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$sce', 'emCasino', ctrlFactory]);
    function ctrlFactory($scope, $sce, emCasino) {
        $scope.games = [];
        $scope.gameCategories = [];
        $scope.mostPlayedGames = [];
        $scope.gameLaunchData = null;
        $scope.gameUrl = null;

        $scope.onGameSelected = function(slug) {
            emCasino.getLaunchUrl(slug, null, false)
                .then(function(result) {
                        $scope.gameLaunchData = result;
                        $scope.gameUrl = $sce.trustAsResourceUrl(result.url);
                    },
                    logError);
        };

        function getGameCategories() {
            emCasino.getGameCategories()
                .then(function (result) {
                    $scope.gameCategories = result.categories;
                }, logError);
        };

        function getMostPlayedGames() {
            emCasino.getMostPlayedGames(emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail)
                .then(function (result) {
                    $scope.mostPlayedGames = result.games;
                }, logError);
        };

        function getGames() {
            emCasino.getGames({
                filterByVendor:['NetEnt'],
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize:4
                })
                .then(function(result) {
                        $scope.games = result.games;
                    },
                    logError);
        };

        function logError(error) {
            console.log(error);
        };

        function init() {
            getGameCategories();
            getMostPlayedGames();
            getGames();
        };

        init();
    }
})();




//(function () {
//    'use strict';
//    var ctrlId = 'games2Ctrl';
//    APP.controller(ctrlId, ['$scope', '$sce', 'emCasino', ctrlFactory]);
//    function ctrlFactory($scope, $sce, emCasino) {
//        $scope.games = [];
//        $scope.gameCategories = [];
//        $scope.mostPlayedGames = [];
//        $scope.gameLaunchData = null;
//        $scope.gameUrl = null;

//        $scope.onCategoryChanged = function (category) {
//            emCasino.getGames({
//                filterByCategory: [category],
//                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
//                pageSize: 4
//            })
//                .then(function (result) {
//                    $scope.games = result.games;
//                },
//                    logError);
//        };

//        $scope.onGameSelected = function (slug) {
//            emCasino.getLaunchUrl(slug, null, false)
//                .then(function (result) {
//                    $scope.gameLaunchData = result;
//                    $scope.gameUrl = $sce.trustAsResourceUrl(result.url);
//                },
//                    logError);
//        };

//        function getGameCategories() {
//            emCasino.getGameCategories()
//                .then(function (result) {
//                    $scope.gameCategories = result.categories;
//                }, logError);
//        };

//        function getMostPlayedGames() {
//            emCasino.getMostPlayedGames(emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail)
//                .then(function (result) {
//                    $scope.mostPlayedGames = result.games;
//                }, logError);
//        };

//        function logError(error) {
//            console.log(error);
//        };

//        function init() {
//            getGameCategories();
//            getMostPlayedGames();
//            $scope.onCategoryChanged("VIDEOPOKERS");
//        };

//        init();
//    }
//})();