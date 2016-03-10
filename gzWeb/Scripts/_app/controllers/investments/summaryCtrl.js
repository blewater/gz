(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', '$http', ctrlFactory]);
    function ctrlFactory($scope, $http) {
        $scope.vintages = [
            { year: 2016, month: 'March', invested: 150, returnPercent: 10 },
            { year: 2016, month: 'February', invested: 80, returnPercent: 15 },
            { year: 2016, month: 'January', invested: 200, returnPercent: -5 },
            { year: 2015, month: 'December', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'November', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'October', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'September', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'August', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'July', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'June', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'May', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'April', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'March', invested: 100, returnPercent: 10 },
            { year: 2015, month: 'February', invested: 10, returnPercent: 10 },
            { year: 2015, month: 'January', invested: 10, returnPercent: 10 },
            { year: 2014, month: 'December', invested: 100, returnPercent: 10 },
            { year: 2014, month: 'November', invested: 100, returnPercent: 10 },
            { year: 2014, month: 'October', invested: 100, returnPercent: 10 },
            { year: 2014, month: 'September', invested: 100, returnPercent: 10 },
            { year: 2014, month: 'August', invested: 100, returnPercent: 10 },
            { year: 2014, month: 'July', invested: 100, returnPercent: 10 }
        ];
        $scope.lastVintages = $scope.vintages.slice(0, 3);
        $scope.showAllVintages = function () { };
        $scope.transferCashToGames = function () { };

        //function init() {
            
        //}
        //init();

        //$http.post('/Mvc/Auth/SignOut', data).success(function (response) {
        //    navService.setRequestUrl('/');
        //    localStorageService.remove('randomSuffix');
        //    var templates = $filter('toArray')(constants.templates);
        //    for (var i = 0; i < templates.length; i++)
        //        $templateCache.remove(templates[i]);
        //    notificationsService.stop();
        //    messageService.clear();

        //    deferred.resolve(response);
        //});
    }
})();