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
                    for (var i = 0; i < $scope.paymentMethods.length; i++)
                        $scope.paymentMethods[i].displayName = emBanking.getPaymentMethodDisplayName($scope.paymentMethods[i]);
                    $scope.initializing = false;
                }, function (error) {
                    message.autoCloseError(error.desc);
                    $scope.initializing = false;
                });
            }, function (error) {
                message.autoCloseError(error.desc);
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