(function () {
    'use strict';
    var ctrlId = 'depositCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emBanking', 'iso4217', ctrlFactory]);
    function ctrlFactory($scope, $filter, emBanking, iso4217) {
        var thisYear = moment().year();
        var maxYear = thisYear + 30;

        $scope.model = {
            id: undefined,
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

        $scope.onCreditCardSelected = function (card) {
            $scope.model.id = card.id;
            $scope.model.cardNumber = card.name;
            $scope.model.cardHolderName = card.cardHolderName;
            $scope.model.cardExpiryYear = parseInt(card.cardExpiryDate.slice(0, 2));
            $scope.model.cardExpiryMonth = parseInt(card.cardExpiryDate.slice(0, -4));
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

        function getFields() {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency.code,
                amount: $scope.model.amount,
                payCardID: $scope.model.id
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            if ($scope.model.id)
                q.resolve(getFields());
            else {
                emBanking.registerPayCard({
                    paymentMethodCode: $scope.selectedMethod,
                    fields: {
                        cardNumber: $scope.model.cardNumber,
                        cardHolderName: $scope.model.cardHolderName,
                        cardExpiryDate: $scope.model.cardExpiryMonth + "/" + $scope.model.cardExpiryYear
                    }
                }).then(function(result) {
                    $scope.model.id = result.id;
                    q.resolve(getFields());
                }, function (error) {
                    q.reject(error);
                });
            }
            return q.promise;
        }

        init();
    }
})();