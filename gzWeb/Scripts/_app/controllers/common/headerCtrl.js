(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', '$location', 'constants', 'route', 'emWamp', 'message', ctrlFactory]);
    function ctrlFactory($scope, $location, constants, route, emWamp, message) {
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

        emWamp.getSessionInfo().then(function (response) {
            $scope.isAuthenticated = response.isAuthenticated;
            $scope.initials = $scope.isAuthenticated ? response.firstname.substring(0, 1) + response.surname.substring(0, 1) : '';
            $scope.name = $scope.isAuthenticated ? response.firstname : '';
        });

        $scope.showLogin = function () {
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
    }
})();