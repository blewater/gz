(function () {
    'use strict';
    var ctrlId = 'promotionsCtrl';
    APP.controller(ctrlId, ['$scope', 'api', ctrlFactory]);
    function ctrlFactory($scope, api) {
        function loadThumbnails() {
            api.call(function () {
                return api.getThumbnails();
            }, function (response) {
                $scope.thumbnails = response.Result;
            });
        };
        function init() {
            loadThumbnails();
        };
        init();

        $scope.getPageUrl = function (code) {
            return constants.routes.promotion.path.replace(":code", code) + $location.url().substring($location.path().length);
        }
    }
})();
