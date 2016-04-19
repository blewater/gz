(function() {
    "use strict";

    APP.factory("emBanking", ["$q", "emWamp", emBankingFunc]);

    function emBankingFunc($q, emWamp) {

        var _service = {};

        var _supportedPaymentMethodCode = {
            VISA: "VISA",
            Maestro: "Maestro",
            MasterCard: "MasterCard"
        };

        _service.PaymentMethodCode = _supportedPaymentMethodCode;

        /// <summary>
        /// Query the gaming accounts of the current logged-in user.
        /// </summary>
        /// <parameter name="expectBalance">
        /// Indicates if balance is expected in the response with the account. 
        /// NOTE, Loading balance is always a heavy operation, avoid loading balance as possible as much.
        /// </parmater>
        /// <parameter name="expectBonus">
        /// Indicates if bonus account is expected in the response. If expectBonus is true, expectBalance has to be true.
        /// </parmater>
        _service.getGamingAccounts = function(expectBalance, expectBonus) {
            return emWamp.call("/user/account#getGamingAccounts",
            {
                expectBalance: expectBalance,
                expectBonus: expectBonus
            });
        };

        //
        // Query the payment methods.
        //
        // filterByCountry  [string, optional]
        // Filter the payment method against country. ISO3166 Alpha2.
        // currency  [string, optional]
        // The preferred currency for limitation. ISO 4217 code.
        _service.getPaymentMethods = function(filterByCountry, currency) {
            return emWamp.call("/user/deposit#getPaymentMethods",
            { filterByCountry: filterByCountry, currency: currency });
        };

        _service.getSupportedPaymentMethods = function(filterByCountry, currency) {
            var q = $q.defer();
            emWamp.call("/user/deposit#getPaymentMethods", { filterByCountry: filterByCountry, currency: currency })
                .then(function(result) {
                    var paymentMethods = [];
                    angular.forEach(_supportedPaymentMethodCode,
                        function(key, value) {
                            paymentMethods.push(result.paymentMethods[key]);
                        });
                    q.resolve(paymentMethods);
                });
            return q.promise;
        };

        //
        // Query the payment method grouped by categories.
        //
        // filterByCountry  [string, optional]
        // Filter the payment method against country. ISO3166 Alpha2.
        // currency  [string, optional]
        // The preferred currency for limitation. ISO 4217 code.
        _service.getCategorizedPagmentMethods = function(filterByCountry, currency) {
            return emWamp.call("/user/deposit#getCategorizedPagmentMethods",
            { filterByCountry: filterByCountry, currency: currency });
        };

        //
        // Query the recent used payment methods for the current logged-in user.
        //
        // currency  [string, optional]
        // The preferred currency for limitation. ISO 4217 code.
        _service.getRecentUsedPaymentMethods = function(currency) {
            return emWamp.call("/user/deposit#getRecentUsedPaymentMethods", { currency: currency });
        };

        //
        // Query the configuration of payment method for deposit
        //
        // paymentMethodCode  [string, mandatory]
        // The identifier of the payment method, can be gotten from getCategorizedPagmentMethods.
        _service.getPaymentMethodCfg = function(paymentMethodCode) {
            return emWamp.call("/user/deposit#getPaymentMethodCfg", { paymentMethodCode: paymentMethodCode });
        };

        //
        // Register pay card for the current logged-in user.
        //
        // Parameter
        // {
        //      "paymentMethodCode": "MasterCard",
        //      "fields": {
        //          // the following fields are different per each payment method
        //          // To get the field requirement for specific payment method, use /user/deposit#getPaymentMethodCfg
        //
        //          "cardNumber": "5138495125550554",
        //          "cardHolderName": "5138495125550554",
        //          "cardExpiryDate": "03/2014",
        //          "cardValidFrom": "02/2012",
        //          "cardIssueNumber": "123",
        //          ...
        //      }
        // }
        //
        // paymentMethodCode  [string, mandatory]
        // The identifier of the payment method, can be gotten from getCategorizedPagmentMethods.
        //
        // fields  [JSON object]
        // The field requirement is different per different payment method and user. 
        // Within the response of the call getPaymentMethodCfg, the properties of fields.payCardID.registrationFields 
        // contain the information of the fields to register the paycard for each payment method.
        _service.registerPayCard = function(parameter) {
            return emWamp.call("/user/deposit#registerPayCard", parameter);
        };

        _service.registerPayCardVISA = function(cardNumber, cardHolderName, cardExpiryDate) {
            return emWamp.call("/user/deposit#registerPayCard",
            {
                paymentMethodCode: _supportedPaymentMethodCode.VISA,
                fields: {
                    cardNumber: cardNumber,
                    cardHolderName: cardHolderName,
                    cardExpiryDate: cardExpiryDate
                }
            });
        };

        _service.registerPayCardMaestro = function(cardNumber,
            cardHolderName,
            cardExpiryDate,
            cardValidFrom,
            cardIssueNumber) {
            return emWamp.call("/user/deposit#registerPayCard",
            {
                paymentMethodCode: _supportedPaymentMethodCode.Maestro,
                fields: {
                    cardNumber: cardNumber,
                    cardHolderName: cardHolderName,
                    cardExpiryDate: cardExpiryDate,
                    cardValidFrom: cardValidFrom,
                    cardIssueNumber: cardIssueNumber
                }
            });
        };

        _service.registerPayCardMasterCard = function(cardNumber,
            cardHolderName,
            cardExpiryDate,
            cardValidFrom,
            cardIssueNumber) {
            return emWamp.call("/user/deposit#registerPayCard",
            {
                paymentMethodCode: _supportedPaymentMethodCode.MasterCard,
                fields: {
                    cardNumber: cardNumber,
                    cardHolderName: cardHolderName,
                    cardExpiryDate: cardExpiryDate,
                    cardValidFrom: cardValidFrom,
                    cardIssueNumber: cardIssueNumber
                }
            });
        };

        //
        // Prepare the deposit transaction for the current logged-in user.
        //
        // parameters
        // {
        //    "paymentMethodCode": "VISA",
        // 
        //    // the following fields are different per each payment method
        //    // the field requirement for each payment method can be queried from /user/deposit#getPaymentMethodCfg method
        //    "fields":
        //    {
        //        "gamingAccountID": 429088,
        //        "currency": "CNY",
        //        "amount": "100",
        //        "payCardID": "330126",
        //        "cardSecurityCode": "123",
        //        "bonusCode": "",
        //    ...
        //        }
        // }
        //
        // paymentMethodCode  [string, mandatory]
        // The identifier of the payment method, can be gotten from getCategorizedPagmentMethods.
        //
        // fields  [JSON object]
        // The field requirement is different per different payment method and user.
        // Using getPaymentMethodCfg to query the required field of the specific payment method.
        //
        _service.prepare = function(parameters) {
            return emWamp.call("/user/deposit#prepare", parameters);
        };

        _service.prepareVisa = function(gamingAccountId, currency, amount, payCardId, cardSecurityCode) {
            return emWamp.call("/user/deposit#prepare",
            {
                paymentMethodCode: _supportedPaymentMethodCode.VISA,
                fields: {
                    gamingAccountID: gamingAccountId,
                    currency: currency,
                    amount: amount,
                    payCardID: payCardId,
                    cardSecurityCode: cardSecurityCode
                }
            });
        };

        _service.prepareMaestro = function(gamingAccountId, currency, amount, payCardId, cardSecurityCode) {
            return emWamp.call("/user/deposit#prepare",
            {
                paymentMethodCode: _supportedPaymentMethodCode.Maestro,
                fields: {
                    gamingAccountID: gamingAccountId,
                    currency: currency,
                    amount: amount,
                    payCardID: payCardId,
                    cardSecurityCode: cardSecurityCode
                }
            });
        };

        _service.prepareMasterCard = function(gamingAccountId, currency, amount, payCardId, cardSecurityCode) {
            return emWamp.call("/user/deposit#prepare",
            {
                paymentMethodCode: _supportedPaymentMethodCode.MasterCard,
                fields: {
                    gamingAccountID: gamingAccountId,
                    currency: currency,
                    amount: amount,
                    payCardID: payCardId,
                    cardSecurityCode: cardSecurityCode
                }
            });
        };

        return _service;
    };

})();