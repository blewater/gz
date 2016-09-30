(function () {
    'use strict';
    var ctrlId = 'promotionCtrl';
    APP.controller(ctrlId, ['$scope', 'api', '$routeParams', '$location', ctrlFactory]);
    function ctrlFactory($scope, api, $routeParams, $location) {
        function loadPage() {
            var code = $routeParams.code;
            api.call(function () {
                return api.getPage(code);
            }, function (response) {
                $scope.html = response.Result;
            });
        }

        function init (){
            loadPage();
        };

        init();

        $scope.go = function (path) {
            $location.url(path);
        };
    }
})();
