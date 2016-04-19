(function() {
    "use strict";

    APP.factory("emWamp", ["$wamp", emWampFunction]);
    function emWampFunction($wamp) {

        var _logError = function(error) {
            console.log(error);
        };

        var _call = function(uri, parameters) {
            var callReturn = $wamp.call(uri, [], parameters);

            var originalFunc = callReturn.then;
            callReturn.then = function(successCallback, failureCallback) {
                function success(d) {
                    if (typeof (successCallback) === 'function')
                        successCallback(d && d.kwargs);
                }

                function error(e) {
                    if (typeof (failureCallback) === 'function')
                        failureCallback(e.kwargs);
                }

                return originalFunc.call(callReturn, success, error);
            };

            return callReturn;
        };

        var service = {

            call: _call,

            // #region Account

            register: function(parameters) {
                return _call("/user/account#register", parameters);
            },

            // #endregion

            // #region Session
            /// <summary>
            /// Login the end-user.
            /// </summary>
            /// <parameters>
            /// For clasic login:
            ///     {
            ///         usernameOrEmail: 'xx@xx.com',
            ///         password: 'iampwd',
            ///         captchaPublicKey: "6LcJ7e4SAAAAAOaigpBV8fDtQlWIDrRPNFHjQRqn",
            ///         captchaChallenge: "03AHJ_VutMqFDQyKxHChZ6vF4pi8Zu76IzvP5YCNdZMeOdjVYpY",
            ///         captchaResponse: "120"
            ///     }
            /// For external login:
            ///     {
            ///         "referrerID": "0fbfcca4166149f6a26798d3a2f90a76"
            ///     }
            /// </parameters>
            login: function (parameters) {
                return _call("/user#login", parameters);
            },

            /// <summary>
            /// Get current session information.
            /// <summary>
            /// <returns>
            /// {
            ///     "isAuthenticated": true,
            ///     "firstname": "Bruce",
            ///     "surname": "Wliam",
            ///     "currency": "EUR",
            ///     "userCountry": "AS",
            ///     "ipCountry": "ES",
            ///     "loginTime": "2014-01-01T00:00:00",
            ///     "isEmailVerified": true
            ///     }
            /// </returns>
            getSessionInfo: function() {
                return _call("/user#getSessionInfo");
            },

            logout: function() {
                _call("/user#logout").then(function(result) {}, _logError);
            }

            // #endregion
        // 
        };

        $wamp.open();

        return service;
    };

    APP.factory("emCasino", ["emWamp", emCasinoFunc]);
    function emCasinoFunc(emWamp) {

        var _service = {};

        _service.FIELDS = {
            Slug: 1,
            Vendor: 2,
            Name: 4,
            ShortName: 8,
            Description: 16,
            AnonymousFunMode: 32,
            FunMode: 64,
            RealMode: 128,
            NewGame: 256,
            License: 512,
            Popularity: 1024,
            Width: 2048,
            Height: 4096,
            Thumbnail: 8192,
            Logo: 16384,
            BackgroundImage: 32768,
            Url: 65536,
            HelpUrl: 131072,
            Categories: 262144,
            Tags: 524288,
            Platforms: 1048576,
            RestrictedTerritories: 2097152,
            TheoreticalPayOut: 4194304,
            BonusContribution: 8388608,
            JackpotContribution: 16777216,
            FPP: 33554432,
            Limitation: 536870912,
            Currencies: 8589934592,
            Languages: 17179869184
        };

        _service.getGameCategories = function() {
            return emWamp.call("/casino#getGameCategories");
        };

        /// <summary>
        /// Query the casino games according to specific condition indicated in input parameters.
        /// </summary>
        /// <parameter name="filterByName">
        /// [string array, optional]
        /// Filter the games against game name. Only the games whose name exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterBySlug">
        /// [string array, optional]
        /// Filter the games against game slug, the unique identity of the game. Only the games whose slug exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByVendor">
        /// [string array, optional]
        /// Filter the games against vendors. Only the games whose vendor exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByCategory">
        /// [string array, optional]
        /// Filter the games against categories. Only the games whose category exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByTag">
        /// [string array, optional]
        /// Filter the games against tags. Only the games whose tag exists in this array are returned
        /// </parameter>
        /// <parameter name="filterByAttribute">
        /// [JSON object]
        /// Filter the games against attributes. Only the below attributes are supported:
        /// <code>
        /// {
        ///     "newGame": true //true or false, only new / non-new games are returned
        /// }
        /// </code>
        /// </parameter>
        /// <parameter name="expectedFields">
        /// [integer, mandatory]
        /// To reduce the size of the returned data for the method, this parameter is used to indicate the game fields expected in JSON response. 
        /// Individual field value is defined as below. Passing this field with the sum of the expected fields' value.
        /// </parameter>
        //_service.getGames = function (filterByName, filterBySlug, filterByVendor, filterByCategory, filterByTag, filterByAttribute, expectedFields, pageIndex, pageSize) {
        //    //angular.forEach(ex)
        //    return emWamp.call("/casino#getGames",
        //    {
        //        filterByName: filterByName,
        //        filterBySlug: filterBySlug,
        //        filterByVendor: filterByVendor,
        //        filterByCategory: filterByCategory,
        //        filterByTag: filterByTag,
        //        filterByAttribute: filterByAttribute,
        //        expectedFields: expectedFields,
        //        expectedFormat: "array",
        //        pageIndex: pageIndex,
        //        pageSize: pageSize,
        //    });
        //};

        _service.getGameCategories = function () {
            return emWamp.call("/casino#getGameCategories");
        };

        _service.getGames = function(parameters) {
            return emWamp.call("/casino#getGames", parameters);
        };

        _service.getGameVendors = function () {
            return emWamp.call("/casino#getGameVendors");
        };

        return _service;
    };

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
        _service.getGamingAccounts = function (expectBalance, expectBonus) {
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
        _service.getPaymentMethods = function (filterByCountry, currency) {
            return emWamp.call("/user/deposit#getPaymentMethods", { filterByCountry: filterByCountry, currency: currency });
        };

        _service.getSupportedPaymentMethods = function (filterByCountry, currency) {
            var q = $q.defer();
            emWamp.call("/user/deposit#getPaymentMethods", { filterByCountry: filterByCountry, currency: currency })
                .then(function (result) {
                    var paymentMethods = [];
                    angular.forEach(_supportedPaymentMethodCode, function (key, value) {
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
        _service.getCategorizedPagmentMethods = function (filterByCountry, currency) {
            return emWamp.call("/user/deposit#getCategorizedPagmentMethods", { filterByCountry: filterByCountry, currency: currency });
        };

        //
        // Query the recent used payment methods for the current logged-in user.
        //
        // currency  [string, optional]
        // The preferred currency for limitation. ISO 4217 code.
        _service.getRecentUsedPaymentMethods = function (currency) {
            return emWamp.call("/user/deposit#getRecentUsedPaymentMethods", { currency: currency });
        };

        //
        // Query the configuration of payment method for deposit
        //
        // paymentMethodCode  [string, mandatory]
        // The identifier of the payment method, can be gotten from getCategorizedPagmentMethods.
        _service.getPaymentMethodCfg = function (paymentMethodCode) {
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

        _service.registerPayCardMaestro = function (cardNumber, cardHolderName, cardExpiryDate, cardValidFrom, cardIssueNumber) {
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

        _service.registerPayCardMasterCard = function (cardNumber, cardHolderName, cardExpiryDate, cardValidFrom, cardIssueNumber) {
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
        _service.prepare = function (parameters) {
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

        _service.prepareMasterCard = function (gamingAccountId, currency, amount, payCardId, cardSecurityCode) {
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