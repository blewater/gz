(function () {
    'use strict';
    var ctrlId = 'games1Ctrl';
    APP.controller(ctrlId, ['$scope', 'emCasino', ctrlFactory]);
    function ctrlFactory($scope, emCasino) {
        $scope.gameCategories = [];
        $scope.mostPlayedGames = [];

        function getGameCategories() {
            emCasino.getGameCategories()
                .then(function (result) {
                    $scope.gameCategories = result.categories;
                }, logError);
        }

        function getMostPlayedGames() {
            emCasino.getMostPlayedGames(emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.BackgroundImage)
                .then(function (result) {
                    $scope.mostPlayedGames = result.games;
                }, logError);
        }

        function getGames() {
            
        }

        function logError(error) {
            console.log(error);
        }

        function init() {
            getGameCategories();
            getMostPlayedGames();
        };

        init();
    }
})();