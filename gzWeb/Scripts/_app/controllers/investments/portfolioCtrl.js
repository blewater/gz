﻿(function () {
    'use strict';
    var ctrlId = 'portfolioCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope.model = {
            nextInvestmentOn: 'On January 31st',
            plans: [
                {
                    title: 'Aggressive',
                    balance: 1500,
                    selected: false,
                    holdings: [
                        { name: 'Vanguard VTI', weight: 8.00 },
                        { name: 'Vanguard VEA', weight: 5.00 },
                        { name: 'Vanguard VWO', weight: 5.00 },
                        { name: 'Vanguard VIG', weight: 15.00 },
                        { name: 'State Street XLE', weight: 7.00 },
                        { name: 'Schwab SCHP', weight: 25.00 },
                        { name: 'State Street XLE', weight: 35.00 }
                    ]
                },
                {
                    title: 'Moderate',
                    balance: 1500,
                    selected: true,
                    holdings: [
                        { name: 'Vanguard VTI', weight: 8.00 },
                        { name: 'Vanguard VEA', weight: 5.00 },
                        { name: 'Vanguard VWO', weight: 5.00 },
                        { name: 'Vanguard VIG', weight: 15.00 },
                        { name: 'State Street XLE', weight: 7.00 },
                        { name: 'Schwab SCHP', weight: 25.00 },
                        { name: 'State Street XLE', weight: 35.00 }
                    ]
                },
                {
                    title: 'Conservative',
                    balance: 1500,
                    selected: false,
                    holdings: [
                        { name: 'Vanguard VTI', weight: 8.00 },
                        { name: 'Vanguard VEA', weight: 5.00 },
                        { name: 'Vanguard VWO', weight: 5.00 },
                        { name: 'Vanguard VIG', weight: 15.00 },
                        { name: 'State Street XLE', weight: 7.00 },
                        { name: 'Schwab SCHP', weight: 25.00 },
                        { name: 'State Street XLE', weight: 35.00 }
                    ]
                }
            ],
            conservativePercent: 43,
            moderatePercent: 23,
            aggressivePercent: 34,
            averagePercent: 59
        }
        $scope.selectPlan = function(plan) {
            var index = $scope.model.plans.indexOf(plan);
            for (var i = 0; i < $scope.model.plans.length; i++)
                $scope.model.plans[i].selected = index === i;
        }
    }
})();