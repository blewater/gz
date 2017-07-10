(function () {
    'use strict';
    var ctrlId = 'carouselVideoCtrl';
    APP.controller(ctrlId, ['$scope', '$sce', ctrlFactory]);
    function ctrlFactory($scope, $sce) {
        $scope.src = $sce.trustAsResourceUrl($scope.url);
    }
})();
