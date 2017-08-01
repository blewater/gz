(function () {
    'use strict';
    var ctrlId = 'summaryVintagesCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', '$timeout', 'helpers', '$sce', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $filter, $timeout, helpers, $sce, iso4217) {
        $scope.toggleExpandCollapse = function (group) {
            if (group.isCollapsed) {
                group.hide = !group.hide;
                $timeout(function() {
                    group.isCollapsed = !group.isCollapsed;
                }, 10);
            } else {
                group.isCollapsed = !group.isCollapsed;
                $timeout(function () {
                    group.hide = !group.hide;
                }, 400);
            }
        }

        function getFlattened() {
            return $filter('flatten')($scope.vintagesPerYear);
        }

        $scope.thereIsNoSelectedVintage = function() {
            return !helpers.array.any(getFlattened(), function (v) {
                return v.Selected === true && v.Sold === false;
            });
        };

        $scope.calcTotalGainsAndFees = function () {
            var flattened = getFlattened();
            var selected = $filter('where')(flattened, { 'Selected': true, 'Sold': false });

            $scope.totalGain = helpers.array.aggregate(selected, 0, function (s, v) {
                return s + v.SellingValue;
            });
            var feesAmount = helpers.array.aggregate(selected, 0, function (s, v) {
                return s + v.SoldFees;
            });
            var feesText = calcFeesText(feesAmount);
            $scope.totalFeesTooltip = $sce.trustAsHtml(
                '<div class="row">' +
                    '<div class="col-xs-12 text-center">Total fees amount: ' + feesText + '</div>' +
                '</div>' +
                '<div class="row">' +
                    '<div class="col-xs-10 text-left">(greenzorro fee</div>' +
                    '<div class="col-xs-2 text-right">1.5%</div>' +
                '</div>' +
                '<div class="row">' +
                    '<div class="col-xs-10 text-left">+ETF Fund fee</div>' +
                    '<div class="col-xs-2 text-right">2.5%</div>' +
                '</div>' +
                '<div class="row">' +
                    '<div class="col-xs-10 text-left">=fees</div>' +
                    '<div class="col-xs-2 text-right">4.0%)</div>' +
                '</div>'
            );
        };

        //$scope.totalGain = function () {
        //    var flattened = getFlattened();
        //    var selected = $filter('where')(flattened, { 'Selected': true, 'Sold': false });

        //    var feesAmount = helpers.array.aggregate(selected, 0, function (s, v) {
        //        return s + v.SoldFees;
        //    });
        //    calcTotalFeesTooltip(feesAmount);
        //    return helpers.array.aggregate(selected, 0, function (s, v) {
        //        return s + v.SellingValue;
        //    });
        //};

        function calcFeesText(feesAmount) {
            if (feesAmount === 0)
                return "N/A";
            else if (feesAmount > 0 && feesAmount < 1)
                return iso4217.getCurrencyByCode($scope.currency).symbol + "<1";
            else
                return $filter('isoCurrency')(feesAmount, $scope.currency, 0);
        };
        //function calcTotalFeesTooltip(feesAmount) {
        //    var feesText = calcFeesText(feesAmount);
        //    $scope.totalFeesTooltip = $sce.trustAsHtml(
        //        '<div class="row">' +
        //            '<div class="col-xs-12 text-center">Total fees amount: ' + feesText + '</div>' +
        //        '</div>' +
        //        '<div class="row">' +
        //            '<div class="col-xs-10 text-left">(greenzorro fee</div>' +
        //            '<div class="col-xs-2 text-right">1.5%</div>' +
        //        '</div>' +
        //        '<div class="row">' +
        //            '<div class="col-xs-10 text-left">+ETF Fund fee</div>' +
        //            '<div class="col-xs-2 text-right">2.5%</div>' +
        //        '</div>' +
        //        '<div class="row">' +
        //            '<div class="col-xs-10 text-left">=fees</div>' +
        //            '<div class="col-xs-2 text-right">4.0%)</div>' +
        //        '</div>'
        //    );
        //};
        $scope.withdraw = function () {
            var flattened = getFlattened();
            for (var i = 0; i < flattened.length; i++)
                flattened[i].Selected = flattened[i].Selected && !flattened[i].Sold;
            $scope.nsOk(flattened);
        };

        function init() {
            for (var i = 0; i < $scope.vintages.length; i++)
                $scope.vintages[i].Selected = $scope.vintages[i].Sold;
            var grouped = $filter('groupBy')($scope.vintages, 'Year');
            var array = $filter('toArray')(grouped, true);
            var result = $filter('orderBy')(array, '$key', true);
            $scope.vintagesPerYear = result;
            $scope.calcTotalGainsAndFees();
        };
        init();
    }
})();