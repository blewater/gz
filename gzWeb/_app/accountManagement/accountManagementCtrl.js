(function () {
    'use strict';
    var ctrlId = 'accountManagementCtrl';
    APP.controller(ctrlId, ['$scope', 'accountManagement', 'auth', ctrlFactory]);
    function ctrlFactory($scope, accountManagement, auth) {
        $scope.states = accountManagement.states.all;
        $scope.selectorId = accountManagement.selectorId;
        $scope.currentState = undefined;
        $scope.setState = function (state) {
            state.action(state, $scope);
            $scope.currentState = state;
        }
        $scope.setState($scope.state || $scope.states[0]);
    }
})();