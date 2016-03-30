(function () {
    'use strict';
    var ctrlId = 'performanceCtrl';
    APP.controller(ctrlId, ['$scope', '$timeout', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $timeout, $filter) {
        $scope.plans = {
            aggressive: {
                title: 'Aggressive',
                returnRate: 0.16,
                selected: true
            },
            moderate: {
                title: 'Moderate',
                returnRate: 0.08,
                selected: false
            },
            conservative: {
                title: 'Conservative',
                returnRate: 0.02,
                selected: false
            }
        }
        $scope.model = {
            countDuration: 0.6,
            currency: '€',
            years: 1,
            projectedValue: 0,
            profit: 0,
            monthlyInvestment: 30,
            plan: $scope.plans.aggressive,
        }
        
        $scope.calc = function () {
            $scope.modelFrom = angular.copy($scope.model);
            
            // var years = Math.floor((Math.random() * 20));
            // $scope.model.years = years;
            // var investment = $scope.model.monthlyInvestment * 12 * years;
            // $scope.model.profit = investment * $scope.model.plan.returnRate;
            // $scope.model.projectedValue = investment + $scope.model.profit;
            
            var investment = $scope.model.monthlyInvestment * 12 * $scope.model.years;
            $scope.model.profit = investment * $scope.model.plan.returnRate;
            $scope.model.projectedValue = investment + $scope.model.profit;
        }
        $scope.calc();

        $scope.selectPlan = function (plan) {
            var plans = $filter('toArray')($scope.plans);
            var index = plans.indexOf(plan);
            for (var i = 0; i < plans.length; i++)
                plans[i].selected = index === i;
            $scope.model.plan = plans[index];
            $scope.calc();
        }

    }
})();