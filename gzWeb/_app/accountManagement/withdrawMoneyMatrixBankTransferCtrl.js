(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixBankTransferCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', '$filter', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, $filter, $controller) {
        $controller('withdrawMoneyMatrixCommonCtrl', { $scope: $scope });

        $scope.loadPayCardSpecificInfo = function () {
            if ($scope.paymentMethodCfg.monitoringScriptUrl) {
                $.get($scope.paymentMethodCfg.monitoringScriptUrl, undefined, angular.noop, "script").fail(function () {
                    message.autoCloseError('Cannot load provided MonitoringScriptUrl');
                });
            }
            if ($scope.paymentMethodCfg.fields.payCardID.registrationFields) {
                $scope.accountNumberRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.PaymentParameterAccountNumber.regularExpression;
                $scope.bankIdRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.PaymentParameterBankId.regularExpression;
                $scope.ownerNameRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.PaymentParameterOwnerName.regularExpression;
            }
        }

        $scope.getExtraModelProperties = function () {
            return {
                accountNumber: undefined,
                bankId: undefined,
                ownerName: undefined
            };
        };
        $scope.getExtraWithdrawFields = function () {
            return {
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null,
            };
        };
        $scope.getRegistrationFields = function () {
            return {
                PaymentParameterAccountNumber: $scope.model.accountNumber,
                PaymentParameterBankId: $scope.model.bankId,
                PaymentParameterOwnerName: $scope.model.ownerName
            };
        };

        $scope._init();
    }
})();