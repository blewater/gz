(function () {
    'use strict';
    var ctrlId = 'depositPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBanking', 'message', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBanking, message, accountManagement) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

        // #region init
        function loadPaymentMethods() {
            $scope.initializing = true;
            emWamp.getSessionInfo().then(function (sessionInfo) {
                $scope.sessionInfo = sessionInfo;
                emBanking.getSupportedPaymentMethods(sessionInfo.userCountry, sessionInfo.currency).then(function (paymentMethods) {
                    $scope.paymentMethods = paymentMethods;
                    $scope.initializing = false;
                }, function (error) {
                    message.error(error.desc);
                    $scope.initializing = false;
                });
            }, function (error) {
                message.error(error.desc);
                $scope.initializing = false;
            });
        }

        function init() {
            loadPaymentMethods();
        };

        init();
        // #endregion

        // #region selectPaymentMethod
        $scope.selectPaymentMethod = function (method) {
            $scope.setState(accountManagement.states.deposit, {
                paymentMethods: $scope.paymentMethods,
                selectedMethod: method
            });
        };
        // #endregion

    }
})();