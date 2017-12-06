﻿(function () {
    'use strict';
    var ctrlId = 'depositMoneyMatrixEnterCashCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$q', 'iso4217', 'auth', 'emBanking', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $q, iso4217, auth, emBanking, $filter) {
        $scope.model = {
            selectedPayCard: undefined,
            amount: undefined,
            bonusCode: undefined
        };

        $scope.onPayCardSelected = function (payCardId) {
            $scope.model.selectedPayCard = $filter('where')($scope.existingPayCards, { 'id': payCardId })[0];
        };

        function loadCreditCardInfo() {
            if ($scope.paymentMethodCfg.monitoringScriptUrl) {
                $.get($scope.paymentMethodCfg.monitoringScriptUrl, undefined, angular.noop, "script").fail(function () {
                    message.autoCloseError('Cannot load provided MonitoringScriptUrl');
                });
            }

            $scope.existingPayCards = $scope.paymentMethodCfg.fields.payCardID.options;
            $scope.maximumPayCards = $scope.paymentMethodCfg.fields.payCardID.maximumPayCards;
            $scope.thereAreExistingPayCards = $scope.existingPayCards.length > 0;
            $scope.canAddNewPayCard = $scope.existingPayCards.length < $scope.maximumPayCards;

            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];

            var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimits.max + ")";
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
            if (!$rootScope.mobile)
                $scope.amountPlaceholder += amountRange;
            $scope.extraValidityCheck = true;
            if ($scope.existingPayCards.length === 1)
                $scope.onPayCardSelected($scope.existingPayCards[0].id);
        }

        function init() {
            loadCreditCardInfo();
        };

        function getFields(options) {
            var fields = {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null,
                bonusCode: $scope.model.bonusCode
            }
            if (options.id)
                fields.payCardID = options.id;
            return fields;
        }

        $scope.readFields = function () {
            var q = $q.defer();
            q.resolve(getFields({
                id: $scope.model.selectedPayCard ? $scope.model.selectedPayCard.id : undefined,
            }));
            return q.promise;
        }

        $scope.readConfirmMessage = function (prepareData) {
            return "Do you want to deposit the amount of " + prepareData.creditAmount + " using " + $scope.selectedMethod.name + "?";
        };

        init();
    }
})();