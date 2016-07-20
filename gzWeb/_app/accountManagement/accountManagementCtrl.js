(function () {
    'use strict';
    var ctrlId = 'accountManagementCtrl';
    APP.controller(ctrlId, ['$scope', 'accountManagement', 'auth', 'helpers', ctrlFactory]);
    function ctrlFactory($scope, accountManagement, auth, helpers) {
        helpers.ui.watchScreenSize($scope);
        //$scope.xs = screenSize.on('xs', function (match) { $scope.xs = match; });
        //$scope.sm = screenSize.on('sm', function (match) { $scope.sm = match; });
        //$scope.md = screenSize.on('md', function (match) { $scope.md = match; });
        //$scope.lg = screenSize.on('lg', function (match) { $scope.lg = match; });
        //$scope.size = screenSize.get();
        //screenSize.on('xs,sm,md,lg', function () {
        //    $scope.size = screenSize.get();
        //});

        $scope.states = accountManagement.states.all;
        $scope.selectorId = accountManagement.selectorId;
        $scope.currentState = undefined;
        $scope.setState = function (state) {
            state.action(state);
            $scope.currentState = state;
        }
        $scope.setState($scope.state || $scope.states[0]);
    }
})();