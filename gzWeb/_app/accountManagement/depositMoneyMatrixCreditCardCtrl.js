(function () {
    'use strict';
    var ctrlId = 'depositMoneyMatrixCreditCardCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emBanking', '$q', 'iso4217', '$timeout', 'constants', 'message', ctrlFactory]);
    function ctrlFactory($scope, $filter, emBanking, $q, iso4217, $timeout, constants, message) {
        $scope.spinnerWhiteAbs = constants.spinners.sm_abs_white;
        var thisYear = moment().year();
        var maxYear = thisYear + 30;

        $scope.cardNumberLoading = true;
        $scope.cardSecurityNumberLoading = true;

        $scope.model = {
            selectedCreditCard: undefined,
            cardHolderName: undefined,
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
                $scope.model.cardHolderName = card.cardHolderName;
                $scope.model.cardExpiryYear = $filter('where')($scope.years, { 'value': parseInt(card.cardExpiryDate.slice(-4)) })[0];
                $scope.model.cardExpiryMonth = $filter('where')($scope.months, { 'value': parseInt(card.cardExpiryDate.slice(0, 2)) })[0];
                angular.element('#cardSecurityNumber').focus();
            } else {
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
            $scope.accountLimits = $scope.paymentMethodCfg.fields.amount.limits[$scope.currency];
            $scope.amountPlaceholder = iso4217.getCurrencyByCode($scope.currency).symbol + " Amount (between " + $scope.accountLimits.min + " and " + $scope.accountLimits.max + ")";
        }

        function embedCDE() {
            var sdkUrl = $scope.paymentMethodCfg.secureFormScriptUrl;
            if (sdkUrl.indexOf('CardTokenization') === -1) {
                message.error('Wrong SecureFormScriptUrl');
                return;
            }
            $.get(sdkUrl, undefined, initializePaymentForm, "script").fail(function () {
                message.error('Cannot load provided SecureFormScriptUrl');
            });
        }

        // Demo implementation https://plnkr.co/edit/7WZI4KZosjSmGmlGihJW?p=preview
        function initializePaymentForm() {
            var css = {
                'height': '22px',
                'width': '100%',
                'font-size': '14px',
                'display': 'block',
                'line-height': '1.42857143',
                'color': '#000',
                'background-color': '#fff',
                'background-image': 'none',
                'border': 'none',
            };

            $scope.paymentForm = new CDE.PaymentForm({
                'card-number': {
                    selector: '#cardNumberContainer',
                    css: css,
                    placeholder: 'card number',
                    format: true
                },
                'card-security-code': {
                    selector: '#cardSecurityNumberContainer',
                    css: css,
                    placeholder: 'CVC',
                    required: true
                }
            });
            $scope.paymentForm.on('error', function (event, errorData) {
                message.error('Cannot load PaymentForm: ' + errorData.ResponseMessage);
            });
            $scope.paymentForm.fields['card-number'].on('status', function (evt, data) {
                //var cssLink = document.createElement("link")
                //cssLink.href = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css";
                //cssLink.rel = "stylesheet";
                //cssLink.type = "text/css";
                //$('iframe#Pan').context.head.appendChild(cssLink);

                $timeout(function () {
                    $scope.cardNumberLoading = false;
                    $scope.cardNumberValidity = data.valid;
                    $scope.extraValidityCheck = $scope.model.existingCard ? $scope.cardSecurityNumberValidity : $scope.cardNumberValidity && $scope.cardSecurityNumberValidity
                }, 0);
            }).on('field_focus', function () {
                $scope.cardNumberFocused = true;
            }).on('field_blur', function () {
                $scope.cardNumberFocused = false;
            });

            $scope.paymentForm.fields['card-security-code'].on('status', function (evt, data) {
                $timeout(function () {
                    $scope.cardSecurityNumberLoading = false;
                    $scope.cardSecurityNumberValidity = data.valid;
                    $scope.extraValidityCheck = $scope.model.existingCard ? $scope.cardSecurityNumberValidity : $scope.cardNumberValidity && $scope.cardSecurityNumberValidity
                }, 0);
            }).on('field_focus', function () {
                $scope.cardSecurityNumberFocused = true;
            }).on('field_blur', function () {
                $scope.cardSecurityNumberFocused = true;
            });
        }

        function init() {
            loadYears();
            loadMonths();
            loadCreditCardInfo();
            embedCDE();
        };

        function getFields(id) {
            return {
                gamingAccountID: $scope.gamingAccount.id,
                currency: $scope.currency,
                amount: $scope.model.amount,
                payCardID: id
            };
        }

        $scope.readFields = function () {
            var q = $q.defer();
            if ($scope.model.existingCard) {
                if (!$scope.paymentForm.fields['card-security-code'].valid)
                    q.reject("Invalid 'Card Security Number'");
                else {
                    $scope.paymentForm.submitCvv({ CardToken: $scope.model.existingCard.cardToken }).then(function (data) {
                        if (data.Success == true) {
                            q.resolve(getFields($scope.model.existingCard.id));
                        } else {
                            q.reject('CVC tokenization failed: ' + data.ResponseMessage);
                        }
                    }, function (data) {
                        q.reject(data.detail ? data.detail : data.ResponseMessage);
                    });
                }
            }
            else {
                if (!$scope.paymentForm.fields['card-number'].valid)
                    q.reject("Invalid 'Card Number'");
                else if (!$scope.paymentForm.fields['card-security-code'].valid)
                    q.reject("Invalid 'Card Security Number'");
                else {
                    $scope.paymentForm.submit().then(function (data) {
                        if (data.Success === true) {
                            emBanking.registerPayCard({
                                paymentMethodCode: $scope.selectedMethod.code,
                                fields: {
                                    cardToken: data.Data.CardToken,
                                    cardHolderName: $scope.model.cardHolderName,
                                    cardExpiryDate: $scope.model.cardExpiryMonth.display + "/" + $scope.model.cardExpiryYear.value,
                                }
                            }).then(function (result) {
                                q.resolve(getFields(result.registeredPayCard.id));
                            }, function (error) {
                                q.reject(error.desc);
                            });
                        } else {
                            q.reject('CVC tokenization failed: ' + data.ResponseMessage);
                        }
                    }, function (data) {
                        q.reject(data.ResponseMessage);
                    });
                }
            }
            return q.promise;
        }

        $scope.readConfirmMessage = function (prepareData) {
            return "Deposit MoneyMatrixCreditCard";
        };

        init();
    }
})();