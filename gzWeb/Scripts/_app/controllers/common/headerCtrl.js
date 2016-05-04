(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$rootScope', '$scope', '$location', 'constants', 'route', 'emWamp', 'message', 'api', '$window', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, $location, constants, route, emWamp, message, api, $window) {
        $scope.routes = {
            guest: route.getGroup(constants.groupKeys.guest),
            games: route.getGroup(constants.groupKeys.games),
            investments: route.getGroup(constants.groupKeys.investments)
        }
        $scope.getClass = function (path) {
            return route.isCurrentPath(path) ? 'focus' : '';
        }

        $scope.backToGames = function () {
            $scope.gamesMode = true;
            $location.path($scope.routes.games[0].path);
        };
        $scope.toInvestments = function () {
            $scope.gamesMode = false;
            $location.path($scope.routes.investments[0].path);
        };

        function updateSessionInfo() {
            var wasAuthenticated = $scope.isAuthenticated;
            emWamp.getSessionInfo().then(function (response) {
                $scope.isAuthenticated = response.isAuthenticated;
                if ($scope.isAuthenticated) {
                    $scope.initials = response.firstname.substring(0, 1) + response.surname.substring(0, 1);
                    $scope.name = response.firstname;
                    if (!wasAuthenticated)
                        $scope.backToGames();
                }
                else if (wasAuthenticated === true)
                    $location.path(route.getPath(constants.routeKeys.home));
            }, function() {
                $scope.isAuthenticated = false;
                $location.path(route.getPath(constants.routeKeys.home));
            });
        }
        updateSessionInfo();

        $scope.login = function () {
            var loginPromise =
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '/partials/messages/login.html',
                    nsCtrl: 'loginCtrl',
                    nsStatic: true,
                });
            loginPromise.then(function() {
                $scope.isAuthenticated = true;
            });
        };
        $scope.logout = function () {
            emWamp.logout();
            api.logout();
        };

        $scope.$on("$wamp.close", function (event, data) {
            //$location.path('/');
            $window.location.href = '/';
        });

        $scope.$on(constants.events.SESSION_STATE_CHANGE, function (event, args) {
            updateSessionInfo();
        });
    }
})();