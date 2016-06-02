(function () {
    'use strict';
    var ctrlId = 'gamesCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', 'emCasino', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, emCasino, constants) {
        $controller('authCtrl', { $scope: $scope });

        $scope.games = [];
        $scope.gameCategories = [];
        $scope.mostPlayedGames = [];
        $scope.gameLaunchData = null;
        $scope.gameUrl = null;
        $scope.playForRealMoney = true;
        
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

        $scope.onGameSelected = function (slug) {
            $location.path(constants.routes.game.path.replace(":slug", slug));
            //emCasino.getGames({
            //    filterBySlug: [slug],
            //    expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.BackgroundImage,
            //    pageSize: 1,
            //}).then(function (result) {
            //    var game = result.games[0];
            //    emCasino.getLaunchUrl(game.slug, null, $scope.playForRealMoney).then(function (result) {
            //        $scope.gameLaunchData = result;
            //        $scope.gameUrl = $sce.trustAsResourceUrl(result.url);
            //    }, logError);
            //}, logError);
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
        
        $scope._init('games', function () {
            getGameCategories();
            //getMostPlayedGames();
            $scope.onCategoryChanged("VIDEOPOKERS");
        });

        function logError(error) {
            console.log(error);
        };
    }
})();
