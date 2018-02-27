(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixAdyenSepaCtrl';
    APP.controller(ctrlId, ['$scope', '$q', 'iso4217', '$filter', '$controller', ctrlFactory]);
    function ctrlFactory($scope, $q, iso4217, $filter, $controller) {
        $controller('withdrawMoneyMatrixCommonCtrl', { $scope: $scope });

        function getPaymentParameterName(obj) {
            var propNameRet = "PaymentParameterAccountNumber";
            var keys = Object.keys(obj);
            for (var k = 0; k < keys.length; k++)
            {
                if (keys[k].toLowerCase().indexOf("paymentparameteraccountnumber") === 0) {
                    propNameRet = keys[k];
                    break;
                } else if (keys[k].toLowerCase().indexOf("paymentparameteriban") === 0) {
                    propNameRet = keys[k];
                    break;
                }
            }
            return propNameRet;
        }
        $scope.loadPayCardSpecificInfo = function () {
            if ($scope.paymentMethodCfg.monitoringScriptUrl) {
                $.get($scope.paymentMethodCfg.monitoringScriptUrl, undefined, angular.noop, "script").fail(function () {
                    message.autoCloseError('Cannot load provided MonitoringScriptUrl');
                });
            }
            if ($scope.paymentMethodCfg.fields.payCardID.registrationFields) {
                var paymentPropName = getPaymentParameterName($scope.paymentMethodCfg.fields.payCardID.registrationFields);
                $scope.ibanRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields[paymentPropName].regularExpression;
            }

            if ($scope.existingPayCards.length === 1)
                $scope.onPayCardSelected($scope.existingPayCards[0].id);
        };

        $scope.getExtraModelProperties = function () {
            return {
                iban: undefined
            };
        };
        $scope.getExtraWithdrawFields = function () {
            return {
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null
            };
        };

        $scope.getRegistrationFields = function () {
            var paymentPropName = getPaymentParameterName($scope.paymentMethodCfg.fields.payCardID.registrationFields);
            return {
                paymentPropName: $scope.model.iban
            };
        };

        $scope.onPayCardSelected = function (payCardId) {
            $scope.selected.method = $filter('where')($scope.existingPayCards, { 'id': payCardId })[0];
            if ($scope.selected.method) {
                $scope.model.iban = $scope.selected.method.name;
                angular.element('#amount').focus();
            }
            else {
                $scope.model.iban = undefined;
                angular.element('#iban').focus();
            }
        };

        $scope._init();
    }
})();