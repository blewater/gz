(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixAdyenSepaCtrl';
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
                $scope.ibanRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.Iban.regularExpression;
            }
        }

        $scope.getExtraModelProperties = function () {
            return {
                iban: undefined
            };
        };
        $scope.getExtraWithdrawFields = function () {
            return {
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null,
            };
        };
        $scope.getRegistrationFields = function () {
            return {
                Iban: $scope.model.iban
            };
        };

        $scope._init();
    }
})();