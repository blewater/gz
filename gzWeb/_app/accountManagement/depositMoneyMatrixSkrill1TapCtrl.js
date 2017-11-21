(function () {
    'use strict';
    var ctrlId = 'depositMoneyMatrixSkrill1TapCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$q', 'iso4217', 'auth', 'emBanking', '$filter', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $q, iso4217, auth, emBanking, $filter) {
        $scope.model = {
            selectedPayCard: undefined,
            accountEmail: undefined,
            amount: undefined,
            reset: undefined,
            bonusCode: undefined
        };

        $scope.onPayCardSelected = function (accountId) {
            $scope.model.selectedPayCard = $filter('where')($scope.existingPayCards, { 'id': accountId })[0];
            if ($scope.model.selectedPayCard) {
                $scope.model.accountEmail = $scope.model.selectedPayCard.name;
                $scope.accountLimitMax = $scope.model.selectedPayCard.displaySpecificFields.SkrillOneTapMaxAmount;
                var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimitMax + ")";
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
                if ($scope.accountLimits.min < $scope.accountLimitMax && !$rootScope.mobile)
                    $scope.amountPlaceholder += amountRange;

                angular.element('#amount').focus();
            } else {
                $scope.model.accountEmail = undefined;
                $scope.accountLimitMax = $scope.accountLimits.max;
                var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimitMax + ")";
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
                if (!$rootScope.mobile)
                    $scope.amountPlaceholder += amountRange;
                angular.element('#accountEmail').focus();
            }
        };

        $scope.setAccountLimits = function () {
            if ($scope.model.reset) {
                $scope.accountLimitMax = $scope.accountLimits.max;
                var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimitMax + ")";
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
                if (!$rootScope.mobile)
                    $scope.amountPlaceholder += amountRange;
            }
            else if ($scope.model.selectedPayCard) {
                $scope.accountLimitMax = $scope.model.selectedPayCard.displaySpecificFields.SkrillOneTapMaxAmount;
                var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimitMax + ")";
                $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
                if ($scope.accountLimits.min < $scope.accountLimitMax && !$rootScope.mobile)
                    $scope.amountPlaceholder += amountRange;
            }
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
            $scope.accountRegex = $scope.paymentMethodCfg.fields.payCardID.registrationFields.SkrillEmailAddress.regularExpression;

            $scope.gamingAccount = $scope.paymentMethodCfg.fields.gamingAccountID.options[0];
            $scope.currency = $scope.gamingAccount.currency;
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.accountLimitMax = $scope.accountLimits.max;
            var amountRange = " (between " + $scope.accountLimits.min + " and " + $scope.accountLimitMax + ")";
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " amount";
            if (!$rootScope.mobile)
                $scope.amountPlaceholder += amountRange;
            $scope.extraValidityCheck = true;
        }

        function init() {
            loadCreditCardInfo();
            //fetchApplicableBonuses();
        };

        function getFields(options) {
            var fields = {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                MonitoringSessionId: window.MMM !== undefined ? window.MMM.getSession() : null,
                SkrillReSetupOneTap: $scope.model.reset,
                bonusCode: $scope.model.bonusCode
            }
            if (options.id)
                fields.payCardID = options.id;
            else
                fields.SkrillEmailAddress = options.email;
            return fields;
        }

        $scope.readFields = function () {
            var q = $q.defer();
            q.resolve(getFields({
                id: $scope.model.selectedPayCard ? $scope.model.selectedPayCard.id : undefined,
                email: $scope.model.accountEmail
            }));
            return q.promise;
        }

        $scope.readConfirmMessage = function (prepareData) {
            return "Do you want to deposit the amount of " + prepareData.creditAmount + " using " + $scope.selectedMethod.name + "?";
        };

        init();

        //function fetchApplicableBonuses() {
        //    $scope.fetchingBonuses = true;
        //    auth.getApplicableBonuses({
        //        type: 'deposit',
        //        gamingAccountID: auth.data.gamingAccounts[0].id
        //    }).then(function (result) {
        //        $scope.fetchingBonuses = false;
        //        $scope.enableBonusInput = result.enableBonusInput;
        //        $scope.enableBonusSelector = result.enableBonusSelector;
        //        $scope.applicableBonuses = result.bonuses;
        //    }, function (error) {
        //        $scope.fetchingBonuses = false;
        //        message.error(error.desc);
        //    });
        //}
    }
})();