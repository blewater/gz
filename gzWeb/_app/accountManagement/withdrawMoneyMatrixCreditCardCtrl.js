﻿(function () {
    'use strict';
    var ctrlId = 'withdrawMoneyMatrixCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emBanking', '$q', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $filter, emBanking, $q, iso4217) {
        $scope.model = {
            existingCard: $scope.selectedMethod.payCard,
            amount: undefined,
        };

        function loadCreditCardInfo() {
            $scope.payCardID = $scope.paymentMethodCfg.fields.payCardID.options[0];
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount";
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
        }

        function init() {
            loadCreditCardInfo();
        };

        function getFields(id) {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                payCardID: $scope.payCardID.id,
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            //if ($scope.model.existingCard){
                q.resolve(getFields($scope.model.existingCard.id));
            //}
            //else {
            //    emBanking.registerPayCard({
            //        paymentMethodCode: $scope.selectedMethod.code,
            //        fields: {
            //            cardNumber: $scope.model.cardNumber,
            //            cardHolderName: $scope.model.cardHolderName,
            //            cardExpiryDate: $scope.model.cardExpiryMonth.display + "/" + $scope.model.cardExpiryYear.value
            //        }
            //    }).then(function(result) {
            //        q.resolve(getFields(result.registeredPayCard.id));
            //    }, function (error) {
            //        q.reject(error);
            //    });
            //}
            return q.promise;
        }

        init();
    }
})();