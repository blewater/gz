(function () {
    'use strict';
    var ctrlId = 'registerPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', 'message', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, message) {        
        // #region steps
        $scope.currentStep = 2;
        $scope.steps = [
            { title: "Account Details" },
            { title: "Personal Details" },
            { title: "Deposit" }
        ];
        // #endregion

        // #region init
        function loadPaymentMethods() {
            emWamp.getSessionInfo().then(function(sessionInfo) {
                emBanking.getSupportedPaymentMethods(sessionInfo.userCountry, sessionInfo.currency).then(function (paymentMethods) {
                    $scope.paymentMethods = paymentMethods;
                }, function(error) {
                    console.log(error.desc);
                });
            }, function(error) {
                console.log(error.desc);
            });
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
                    paymentMethod: method
                }
            });
        };
        // #endregion

        // #region gotoGames
        $scope.gotoGames = function () {
            $scope.nsOk(false);
        };
        // #endregion

        init();
    }
})();