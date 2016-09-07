(function () {
    'use strict';
    var ctrlId = 'registerPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', 'constants', '$location', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, constants, $location) {
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        // #region steps
        $scope.currentStep = 2;
        $scope.steps = [
            { title: "Account Details" },
            { title: "Personal Details" },
            { title: "Deposit" }
        ];
        // #endregion

        $scope.accountModel = $scope.accountModel || undefined;
        $scope.sessionInfo = $scope.sessionInfo || undefined;
        $scope.paymentMethods = $scope.paymentMethods || undefined;
        $scope.gamingAccounts = $scope.gamingAccounts || undefined;

        // #region init
        function loadPaymentMethods() {
            if (!$scope.sessionInfo) {
                $scope.initializing = true;
                emWamp.getSessionInfo().then(function (sessionInfo) {
                    $scope.sessionInfo = sessionInfo;
                    if (!$scope.paymentMethods) {
                        emBanking.getSupportedPaymentMethods(sessionInfo.userCountry, sessionInfo.currency).then(function(paymentMethods) {
                            $scope.paymentMethods = paymentMethods;
                            $scope.initializing = false;
                        }, function (error) {
                            console.log(error.desc);
                            $scope.initializing = false;
                        });
                    }
                }, function(error) {
                    console.log(error.desc);
                    $scope.initializing = false;
                });
            }
        }

        function init() {
            loadPaymentMethods();
        };
        // #endregion

        // #region selectPaymentMethod
        $scope.selectPaymentMethod = function (method) {
            $scope.nsNext({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/registerDeposit.html',
                nsCtrl: 'registerDepositCtrl',
                nsStatic: true,
                nsParams: {
                    accountModel: $scope.accountModel,
                    sessionInfo: $scope.sessionInfo,
                    paymentMethods: $scope.paymentMethods,
                    gamingAccounts: $scope.gamingAccounts,
                    selectedMethod: method
                }
            });
        };
        // #endregion

        // #region gotoGames
        $scope.gotoGames = function () {
            $scope.nsOk(false);
            $location.path(constants.routes.games.path).search({});
        };
        // #endregion

        init();
    }
})();