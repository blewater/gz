(function () {
    'use strict';
    var ctrlId = 'depositPaymentMethodsCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'emWamp', 'emBanking', 'message', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, constants, emWamp, emBanking, message, accountManagement) {
        $scope.spinnerWhite = constants.spinners.sm_abs_white;

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
                        emBanking.getSupportedPaymentMethods(sessionInfo.userCountry, sessionInfo.currency).then(function (paymentMethods) {
                            $scope.paymentMethods = paymentMethods;
                            $scope.initializing = false;
                        }, function (error) {
                            message.error(error.desc);
                            $scope.initializing = false;
                        });
                    }
                }, function (error) {
                    message.error(error.desc);
                    $scope.initializing = false;
                });
            }
        }

        function init() {
            loadPaymentMethods();
        };

        init();
        // #endregion

        // #region selectPaymentMethod
        $scope.selectPaymentMethod = function (method) {
            $scope.setState(accountManagement.states.deposit, {
                accountModel: $scope.accountModel,
                sessionInfo: $scope.sessionInfo,
                paymentMethods: $scope.paymentMethods,
                gamingAccounts: $scope.gamingAccounts,
                selectedMethod: method
            });
        };
        // #endregion

    }
})();