(function () {
    'use strict';
    var ctrlId = 'depositCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emBanking', '$q', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $filter, emBanking, $q, iso4217) {
        var thisYear = moment().year();
        var maxYear = thisYear + 30;

        $scope.model = {
            selectedCreditCard: undefined,
            cardNumber: undefined,
            cardHolderName: undefined,
            cardSecurityNumber: undefined,
            cardExpiryMonth: undefined,
            cardExpiryYear: undefined,
            wantBonus: false,
            amount: undefined,
        };

        function loadYears() {
            $scope.loadingYears = true;

            var cardExpiryYear = $scope.cardExpiryYear;
            $scope.years = $filter('map')(
                $filter('getRange')([], maxYear, thisYear),
                function (x) {
                    return {
                        value: x,
                        display: x.toString().slice(-2)
                    };
                });
            if ($scope.years.indexOf(cardExpiryYear) !== -1)
                $scope.cardExpiryYear = cardExpiryYear;
            loadMonths($scope.cardExpiryYear);

            $scope.loadingYears = false;
        }
        $scope.onYearSelected = function (year) {
            loadMonths(year);
        };

        function loadMonths(year) {
            $scope.loadingMonths = true;

            var cardExpiryMonth = $scope.cardExpiryMonth;
            var minMonth = year && year === thisYear ? 12 - moment().month() : 1;
            $scope.months = $filter('map')(
                $filter('getRange')([], 12, minMonth),
                function (x) {
                    return {
                        value: x,
                        display: $filter('pad')(x, 2)
                    };
                });
            if (cardExpiryMonth && $filter('map')($scope.months, function (x) { return x.value; }).indexOf(cardExpiryMonth.value) !== -1)
                $scope.cardExpiryMonth = cardExpiryMonth;

            $scope.loadingMonths = false;
        }

        $scope.onCreditCardSelected = function (cardId) {
            var card = $filter('where')($scope.existingCreditCards, { 'id': cardId })[0];
            $scope.model.existingCard = card;
            if (card) {
                $scope.model.cardNumber = card.name;
                $scope.model.cardHolderName = card.cardHolderName;
                $scope.model.cardExpiryYear = $filter('where')($scope.years, { 'value': parseInt(card.cardExpiryDate.slice(-4)) })[0];
                $scope.model.cardExpiryMonth = $filter('where')($scope.months, { 'value': parseInt(card.cardExpiryDate.slice(0, 2)) })[0];
                angular.element('#cardSecurityNumber').focus();
            } else {
                $scope.model.cardNumber = undefined;
                $scope.model.cardHolderName = undefined;
                $scope.model.cardExpiryYear = undefined;
                $scope.model.cardExpiryMonth = undefined;
                angular.element('#cardNumber').focus();
            }
        };

        function loadCreditCardInfo() {
            $scope.existingCreditCards = $scope.paymentMethodCfg.fields.payCardID.options;
            $scope.maximumPayCards = $scope.paymentMethodCfg.fields.payCardID.maximumPayCards;
            $scope.thereAreExistingCreditCards = $scope.existingCreditCards.length > 0;
            $scope.canAddNewCreditCard = $scope.existingCreditCards.length < $scope.maximumPayCards;
            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount";
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
        }


        function init() {
            loadYears();
            loadMonths();
            loadCreditCardInfo();
        };

        function getFields(id) {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                payCardID: id,
                cardSecurityCode: $scope.model.cardSecurityNumber
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            if ($scope.model.existingCard){
                q.resolve(getFields($scope.model.existingCard.id));
            }
            else {
                emBanking.registerPayCard({
                    paymentMethodCode: $scope.selectedMethod.code,
                    fields: {
                        cardNumber: $scope.model.cardNumber,
                        cardHolderName: $scope.model.cardHolderName,
                        cardExpiryDate: $scope.model.cardExpiryMonth.display + "/" + $scope.model.cardExpiryYear.value
                    }
                }).then(function(result) {
                    q.resolve(getFields(result.registeredPayCard.id));
                }, function (error) {
                    q.reject(error);
                });
            }
            return q.promise;
        }

        init();
    }
})();