(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixSkrillCtrl';
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
                $scope.accountRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.SkrillEmailAddress.regularExpression;
            }
        }

        $scope.getExtraModelProperties = function () {
            return {
                accountEmail: undefined
            };
        };
        $scope.getExtraWithdrawFields = function () {
            return {
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null,
            };
        };
        $scope.getRegistrationFields = function () {
            return {
                SkrillEmailAddress: $scope.model.accountEmail
            };
        };

        if ($scope.selected.group.length === 1)
            $scope.selected.method = $scope.selected.group[0];

        $scope._init();
    }
})();