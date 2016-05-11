(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$rootScope', '$scope', '$location', 'constants', 'emWamp', 'message', 'api', '$window', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, $location, constants, emWamp, message, api, $window) {
        var imgDir = "../../Content/Images/";
        $scope.gamesImgOn = imgDir + "games_white.svg";
        $scope.gamesImgOff = imgDir + "games.svg";
        $scope.investmentsImgOn = imgDir + "diagram_white.svg";
        $scope.investmentsImgOff = imgDir + "diagram.svg";

        $scope.routes = {
            guest: [
                constants.routes.transparency,
                constants.routes.about
            ],
            investment: [
                constants.routes.summary,
                constants.routes.portfolio,
                constants.routes.performance
                //constants.routes.activity
            ]
        }
        $scope.getClass = function (path) {
            return $location.path() === path ? 'focus' : '';
        }

        $scope.backToGames = function () {
            $scope.gamesMode = true;
            $location.path(constants.routes.games.path);
        };
        $scope.toInvestments = function () {
            $scope.gamesMode = false;
            $location.path(constants.routes.summary.path);
        };

        function updateSessionInfo() {
            var wasAuthenticated = $scope.isAuthenticated;
            emWamp.getSessionInfo().then(function (response) {
                $scope.isAuthenticated = response.isAuthenticated;
                if ($scope.isAuthenticated) {
                    $scope.initials = response.firstname.substring(0, 1) + response.surname.substring(0, 1);
                    $scope.name = response.firstname;
                    if (wasAuthenticated === false)
                        $scope.backToGames();
                }
                else if (wasAuthenticated === undefined || wasAuthenticated === true)
                    $location.path(constants.routes.home.path);
            }, function() {
                $scope.isAuthenticated = false;
                $location.path(constants.routes.home.path);
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
                    nsStatic: true
                });
            loginPromise.then(function() {
                $scope.isAuthenticated = true;
            });
        };
        $scope.signup = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerAccount.html',
                nsCtrl: 'registerAccountCtrl',
                nsStatic: true
            });
        };
        $scope.logout = function () {
            emWamp.logout();
            api.logout();
        };

        $scope.deposit = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerPaymentMethods.html',
                nsCtrl: 'registerPaymentMethodsCtrl',
                nsStatic: true
            });
        };

        $scope.$on("$wamp.close", function (event, data) {
            //$location.path('/');
            //$window.location.href = '/';
        });

        $scope.$on(constants.events.SESSION_STATE_CHANGE, function (event, args) {
            updateSessionInfo();
        });
    }
})();