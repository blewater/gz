(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$rootScope', 'constants', 'message', 'auth', 'emBanking', 'localStorageService', 'chat', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $rootScope, constants, message, auth, emBanking, localStorageService, chat, accountManagement) {
        $controller('authCtrl', { $scope: $scope });

        var imgDir = "../../Content/Images/";
        $scope.gamesImgOff = imgDir + "games_green.svg";
        $scope.gamesImgOn = imgDir + "games_white.svg";
        $scope.investmentsImgOff = imgDir + "diagram_green.svg";
        $scope.investmentsImgOn = imgDir + "diagram_white.svg";

        $scope.accountManagementStates = accountManagement.states.menu;
        $scope.gotoState = function (state) {
            accountManagement.open(state);
        };

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

        $scope.gotoHome = function () {
            //if ($scope._authData.isGamer)
            //    $scope.backToGames();
            //else
                $location.path(constants.routes.home.path);
        }
        $scope.backToGames = function () {
            $location.path(constants.routes.games.path);
        };
        $scope.toInvestments = function () {
            if ($scope._authData.isInvestor)
                $location.path(constants.routes.summary.path);
            else {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '/partials/messages/investmentAccessError.html',
                    nsCtrl: 'investmentAccessErrorCtrl',
                    nsStatic: true,
                    // TODO Pass username
                    //nsParams: {
                    //    username: $scope._authData.email
                    //}
                });
                //message.error('Cannot connect to investment');
            }
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
        $scope.changePassword = function() {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/changePassword.html',
                nsCtrl: 'changePasswordCtrl',
                nsStatic: true,
            });
        };

        $scope.deposit = function () {
            accountManagement.open(accountManagement.states.depositPaymentMethods);
            //message.open({
            //    nsType: 'modal',
            //    nsSize: '600px',
            //    nsTemplate: '/partials/messages/registerPaymentMethods.html',
            //    nsCtrl: 'registerPaymentMethodsCtrl',
            //    nsStatic: true
            //});
        };
        $scope.withdraw = function () {
            accountManagement.open(accountManagement.states.withdrawPaymentMethods);
            //message.open({
            //    nsType: 'modal',
            //    nsSize: '600px',
            //    nsTemplate: '/partials/messages/registerPaymentMethods.html',
            //    nsCtrl: 'registerPaymentMethodsCtrl',
            //    nsStatic: true
            //});
        };

        function loadAuthData() {
            $scope.name = $scope._authData.firstname;
            $scope.fullname = $scope._authData.firstname + " " + $scope._authData.lastname;
            if ($scope._authData.isGamer || $scope._authData.isInvestor)
                $scope.initials = $scope._authData.firstname.slice(0, 1) + $scope._authData.lastname.slice(0, 1);

            $scope.hasGamingBalance = $scope._authData.gamingAccount !== undefined;
            if ($scope.hasGamingBalance)
                $scope.gamingBalance = $scope._authData.gamingAccount.amount;
            $scope.currency = $scope._authData.currency;
        }
        
        $scope.$on(constants.events.AUTH_CHANGED, loadAuthData);

        $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);

        $scope._init(loadAuthData);

        $scope.inDebugMode = localStorageService.get(constants.storageKeys.debug);

        $scope.expandCollapseMobileMenu = function(){
            $rootScope.mobileMenuExpanded = !$rootScope.mobileMenuExpanded
            if ($rootScope.mobileMenuExpanded)
                chat.hide()
            else
                chat.show()
        }
    }
})();