(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$rootScope', '$scope', '$location', 'constants', 'message', '$window', 'auth', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, $location, constants, message, $window, auth) {
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

        $scope.login = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true
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
            auth.logout();
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
        $scope.withdraw = function () {
            //message.open({
            //    nsType: 'modal',
            //    nsSize: '600px',
            //    nsTemplate: '/partials/messages/registerPaymentMethods.html',
            //    nsCtrl: 'registerPaymentMethodsCtrl',
            //    nsStatic: true
            //});
        };

        $scope.$on("$wamp.close", function (event, data) {
            //$location.path('/');
            //$window.location.href = '/';
        });

        function updateAuthorizationInfo () {
            $scope.authData = auth.data;
            $scope.name = auth.data.firstname;
            $scope.fullname = auth.data.firstname + " " + auth.data.lastname;
            if ($scope.authData.isGamer || $scope.authData.isInvestor)
                $scope.initials = $scope.authData.firstname.slice(0, 1) + $scope.authData.lastname.slice(0, 1);

            if ($scope.authData.isGamer)
                $scope.gamesMode = true;
            else if ($scope.authData.isInvestor)
                $scope.gamesMode = false;
            else
                $scope.gamesMode = undefined;
        }
        updateAuthorizationInfo();
        $scope.$on(constants.events.AUTH_CHANGED, updateAuthorizationInfo);

        $scope.gamingBalance = auth.data.gamingAccount.amount;
        $scope.currency = auth.data.currency;
        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, function () {
            $scope.gamingBalance = auth.data.gamingAccount.amount;
            $scope.currency = auth.data.currency;
            $scope.$apply();
        });
    }
})();