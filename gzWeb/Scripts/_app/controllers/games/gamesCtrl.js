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
        
        $scope.onCategoryChanged = function (category) {
            emCasino.getGames({
                // filterByVendor:['NetEnt'],
                filterByCategory: [category],
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize: 4
            }).then(function (result) {
                $scope.games = result.games;
            }, logError);
       };

        $scope.onGameSelected = function(slug) {
            emCasino.getLaunchUrl(slug, null, false).then(function(result) {
                $scope.gameLaunchData = result;
                $scope.gameUrl = $sce.trustAsResourceUrl(result.url);
            }, logError);
        };

        function getGameCategories() {
            emCasino.getGameCategories().then(function (result) {
                $scope.gameCategories = result.categories;
            }, logError);
        };

        function getMostPlayedGames() {
            var gameFields = emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail;
            //+ emCasino.FIELDS.BackgroundImage
            //+ emCasino.FIELDS.Logo
            emCasino.getMostPlayedGames(gameFields).then(function (result) {
                $scope.mostPlayedGames = result.games;
            }, logError);
        };

        function getGames() {
            emCasino.getGames({
                expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Thumbnail,
                pageSize:4
            }).then(function(result) {
                $scope.games = result.games;
            }, logError);
        };
        
        function init() {
            getGameCategories();
            getMostPlayedGames();
            $scope.onCategoryChanged("VIDEOPOKERS");
        };

        init();
        
        function logError(error) {
            console.log(error);
        };
    }
})();
