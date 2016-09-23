(function () {
    'use strict';
    var ctrlId = 'promotionCtrl';
    APP.controller(ctrlId, ['$scope', 'api', '$routeParams', ctrlFactory]);
    function ctrlFactory($scope, api, $routeParams) {
        //function loadPage() {
        //    var code = $routeParams.code;
        //    api.call(function () {
        //        return api.getPage(code);
        //    }, function (response) {
        //        $scope.html = response.Result;
        //    });
        //}

        //$scope._init(function () {
        //    loadPage();
        //});
    }
})();
