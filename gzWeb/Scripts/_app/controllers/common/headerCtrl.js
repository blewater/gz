(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$rootScope', '$scope', '$location', 'constants', 'route', 'emWamp', 'message', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, $location, constants, route, emWamp, message) {
        $scope.routes = {
            guest: route.getGroup(constants.groupKeys.guest),
            games: route.getGroup(constants.groupKeys.games),
            investments: route.getGroup(constants.groupKeys.investments)
        }
        $scope.getClass = function (path) {
            return route.isCurrentPath(path) ? 'focus' : '';
        }

        $scope.backToGames = function () {
            $scope.gamesMode = !$scope.gamesMode;
            $location.path($scope.routes.games[0].path);
        };
        $scope.toInvestments = function () {
            $scope.gamesMode = !$scope.gamesMode;
            $location.path($scope.routes.investments[0].path);
        };

        function updateSessionInfo() {
            //$scope.isAuthenticated = true;
            emWamp.getSessionInfo().then(function (response) {
                $scope.isAuthenticated = response.isAuthenticated;
                if ($scope.isAuthenticated) {
                    $scope.initials = response.firstname.substring(0, 1) + response.surname.substring(0, 1);
                    $scope.name = response.firstname;
                }
                else
                    $location.path(route.getPath(constants.routeKeys.home));
            });
        }
        updateSessionInfo();

        $scope.register = function () {
            var promise =
                message.open({
                    nsType: 'modal',
                    nsSize: 'md',
                    nsTemplate: '/partials/messages/registerStart.html',
                    nsCtrl: 'registerStartCtrl',
                    nsStatic: true
                });
            promise.then(function (result) {

            });
        };
        $scope.login = function () {
            var promise =
                message.open({
                    nsType: 'modal',
                    nsSize: 'sm',
                    nsTemplate: '/partials/messages/login.html',
                    nsCtrl: 'loginCtrl',
                    nsStatic: true,
                });
            promise.then(function (result) {

            });
        };

        $scope.$on(constants.events.SESSION_STATE_CHANGE, function (event, args) {
            updateSessionInfo();
        });
    }
})();