(function () {
    'use strict';
    var ctrlId = 'accountManagementCtrl';
    APP.controller(ctrlId, ['$scope', 'accountManagement', 'helpers', 'constants', ctrlFactory]);
    function ctrlFactory($scope, accountManagement, helpers, constants) {
        helpers.ui.watchScreenSize($scope);

        $scope.spinnerWhite = constants.spinners.md_abs_white;

        $scope.elementId = accountManagement.elementId;
        $scope.states = accountManagement.states.all;
        $scope.currentState = undefined;
        $scope.setState = function (state) {
            $scope.changingState = true;
            $scope.currentState = state;
            state.action(state, function () {
                $scope.changingState = false;
            });
        }
        $scope.setState($scope.state || $scope.states[0]);
    }
})();