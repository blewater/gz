(function () {
    'use strict';
    var ctrlId = 'headerCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', '$location', '$rootScope', 'constants', 'message', 'auth', 'emBanking', 'localStorageService', 'chat', 'accountManagement', '$filter', '$sce', 'footerMenu', ctrlFactory]);
    function ctrlFactory($scope, $controller, $location, $rootScope, constants, message, auth, emBanking, localStorageService, chat, accountManagement, $filter, $sce, footerMenu) {
        $controller('authCtrl', { $scope: $scope });

        var imgDir = "../../Content/Images/";
        $scope.gamesImgOff = imgDir + "games_green.svg";
        $scope.gamesImgOn = imgDir + "games_white.svg";
        $scope.investmentsImgOff = imgDir + "diagram_green.svg";
        $scope.investmentsImgOn = imgDir + "diagram_white.svg";

        $scope.accountManagementStates = accountManagement.states.menu;
        $scope.gotoState = function (state) {
            if (state.key === accountManagement.states.logout.key)
                auth.logout()
            else
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
            ]
        }
        $scope.getClass = function (path) {
            return $location.path() === path ? 'focus' : '';
        }

        $scope.gotoHome = function () {
            $location.path(constants.routes.home.path);
        }
        $scope.backToGames = function () {
            window.appInsights.trackEvent("GOTO GAMES", { from: "HEADER" });
            $location.path(constants.routes.games.path).search({});
        };
        $scope.toInvestments = function () {
            if ($scope._authData.isInvestor) {
                window.appInsights.trackEvent("GOTO INVESTMENT", { from: "HEADER" });
                $location.path(constants.routes.summary.path);
            }
            else {
                message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '_app/account/investmentAccessError.html',
                    nsCtrl: 'investmentAccessErrorCtrl',
                    nsStatic: true,
                    // TODO Pass username
                    //nsParams: {
                    //    username: $scope._authData.email
                    //}
                });
            }
        };

        $scope.login = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true
            });
        };
        $scope.signup = function () {
            message.open({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '_app/account/registerAccount.html',
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
                nsTemplate: '_app/account/changePassword.html',
                nsCtrl: 'changePasswordCtrl',
                nsStatic: true,
            });
        };

        $scope.deposit = function () {
            accountManagement.open(accountManagement.states.depositPaymentMethods);
        };
        $scope.withdraw = function () {
            accountManagement.open(accountManagement.states.withdrawPaymentMethods);
        };

        $scope.footerMenu = footerMenu.getMenu();

        function loadAuthData() {
            $scope.name = $scope._authData.firstname;
            $scope.fullname = $scope._authData.firstname + " " + $scope._authData.lastname;
            if ($scope._authData.isGamer || $scope._authData.isInvestor)
                $scope.initials = $scope._authData.firstname.slice(0, 1) + $scope._authData.lastname.slice(0, 1);

            $scope.hasGamingBalance = $scope._authData.gamingBalance !== undefined;
            if ($scope.hasGamingBalance) {
                $scope.gamingBalance = $scope._authData.gamingBalance;
                $scope.gamingBalanceDetails = $sce.trustAsHtml(
                    '<div class="row">' +
                        '<div class="col-xs-8 text-left">Casino Wallet:</div>' +
                        '<div class="col-xs-4 text-right">' + $filter('number')($scope._authData.gamingAccounts[0].amount, 2) + '</div>' +
                    '</div>' +
                    '<div class="row">' +
                        '<div class="col-xs-8 text-left">Casino Wallet Bonus:</div>' +
                        '<div class="col-xs-4 text-right">' + $filter('number')($filter('sum')($filter('map')($scope._authData.gamingAccounts.slice(1), function (acc) { return acc.amount; })), 2) + '</div>' +
                    '</div>'
                );
            }
            $scope.currency = $scope._authData.currency;
        }
        
        $scope.expandCollapseMobileMenu = function(){
            $rootScope.mobileMenuExpanded = !$rootScope.mobileMenuExpanded
            if ($rootScope.mobileMenuExpanded)
                chat.hide()
            else
                chat.show()
        }

        $scope._init(function () {
            loadAuthData();
            $scope.inDebugMode = localStorageService.get(constants.storageKeys.debug);
            $scope.$on(constants.events.AUTH_CHANGED, loadAuthData);
            $scope.$on(constants.events.ACCOUNT_BALANCE_CHANGED, loadAuthData);
            $scope.revealed = true;
        });
    }
})();