(function () {
    'use strict';
    var ctrlId = 'accountManagementCtrl';
    APP.controller(ctrlId, ['$scope', 'accountManagement', 'helpers', 'constants', '$timeout', ctrlFactory]);
    function ctrlFactory($scope, accountManagement, helpers, constants, $timeout) {
        helpers.ui.watchScreenSize($scope);

        $scope.spinnerWhite = constants.spinners.md_abs_white;

        $scope.elementId = accountManagement.elementId;
        $scope.menuStates = accountManagement.states.menu;
        $scope.currentState = undefined;
        $scope.setState = function (state, params) {
            $timeout(function () {
                $scope.changingState = true;
                $scope.currentState = state;
                state.action(state, $scope, params, function () {
                    $scope.changingState = false;
                });
                //window.appInsights.trackPageView("Account Management: " + state.title);
            }, 0);
        }
        $scope.setState($scope.state || $scope.menuStates[0]);
    }
})();