(function () {
    'use strict';
    var ctrlId = 'registerSelectPaymentCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', 'emBanking', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emWamp, emBanking, message, constants) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        
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

        // #region register
        $scope.selectPaymentMethod = function (method) {

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