(function() {
    "use strict";

    APP.factory("emBanking", ["$q", "emWamp", emBankingFunc]);

    function emBankingFunc($q, emWamp) {

        var _service = {};

        var _supportedPaymentMethodCodes = {
            VISA: "VISA",
            Maestro: "Maestro",
            MasterCard: "MasterCard",
            Trustly: "MoneyMatrix_Trustly"
        };

        _service.PaymentMethodCode = _supportedPaymentMethodCodes;

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
        // Query the transaction history for current logged-in user with specific conditions.
        //
        // Parameter
        //
        //  {
        //      "type": "Deposit",
        //      "startTime": "2012-04-04T16:00:00.000Z",
        //      "endTime": "2014-03-13T15:59:00.000Z",
        //      "pageIndex": 1,
        //      "pageSize": 20
        //  }
        //
        // type  [string, mandatory]
        // Indicates the type of transactions to be queried
        //      Deposit : Query successful deposit transactions. (https://help.gammatrix-dev.net/help/deposittransactions.html)
        //      Withdraw : Query withdraw transactions including those are pending or in-progress. (https://help.gammatrix-dev.net/help/withdrawtransactions.html)
        //      Transfer : Query transfer between accounts transactions including those are failed, pending or successful. (https://help.gammatrix-dev.net/help/TransferbetweenAccounts1.html)
        //      BuddyTransfer : Query successful buddy transfer transactions. (https://help.gammatrix-dev.net/help/BuddyTransfer1.html)
        //
        // startTime  [string, mandatory]
        // The start time of the range to filter the transactions. This parameter should be presented in ISO 8601 spec in UTC time.
        //
        // endTime  [string, mandatory]
        // The end time of the range to filter the transactions. This parameter should be presented in ISO 8601 spec in UTC time.
        // 
        // pageIndex  [integer, mandatory]
        // The page index of the result. 1 means first page. Check remark section below for details.
        // 
        // pageSize  [integer, mandatory]
        // The page size of the result. Check remark section below for details.
        //
        // Return
        //  {
        //      "transactions": [ ... ],
        //      "currentPageIndex": 1,
        //      "totalRecordCount": 46,
        //      "totalPageCount": 3
        //  }
        //
        // transactions  [array]
        // An array contains JSON objects, each of them represents an individual transaction record.
        // The format is different per each transaction type. Please click the link for each type above and see details
        //
        // currentPageIndex  [integer]
        // The current page index of the returned records.
        // 
        // totalRecordCount  [integer]
        // The total number of the records for this transaction query.
        // 
        // totalPageCount  [integer]
        // The total number of the pages  for this transaction query.
        //
        _service.getTransactionHistory = function(type, startTime, endTime, pageIndex, pageSize) {
            return emWamp.call("/user#getTransactionHistory",
            {
                type: type,
                startTime: startTime,
                endTime: endTime,
                pageIndex: pageIndex,
                pageSize: pageSize
            });
        };

        _service.watchBalance = function () {
            return emWamp.call("/user/account#watchBalance");
        };

        _service.unwatchBalance = function () {
            return emWamp.call("/user/account#unwatchBalance");
        };

        // #region Deposit

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
                    angular.forEach(_supportedPaymentMethodCodes,
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
        //          // To get the field requirement for specific payment method, use getPaymentMethodCfg
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
        //
        // Return
        //
        //  {
        //      "registeredPayCard": {
        //          "id": 330200,
        //          "name": "633458...0000",
        //          "cardExpiryDate": "03/2015",
        //          "cardHolderName": "6334580500000000"
        //      }
        //  }
        //
        _service.registerPayCard = function(parameter) {
            return emWamp.call("/user/deposit#registerPayCard", parameter);
        };


        _service.registerPayCardVISA = function(cardNumber, cardHolderName, cardExpiryDate) {
            return emWamp.call("/user/deposit#registerPayCard",
            {
                paymentMethodCode: _supportedPaymentMethodCodes.VISA,
                fields: {
                    cardNumber: cardNumber,
                    cardHolderName: cardHolderName,
                    cardExpiryDate: cardExpiryDate
                }
            });
        };;
        _service.registerPayCardMaestro = function(cardNumber, cardHolderName, cardExpiryDate, cardValidFrom, cardIssueNumber) {
            return emWamp.call("/user/deposit#registerPayCard",
            {
                paymentMethodCode: _supportedPaymentMethodCodes.Maestro,
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
                paymentMethodCode: _supportedPaymentMethodCodes.MasterCard,
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

        _service.prepareVISA = function(gamingAccountId, currency, amount, payCardId, cardSecurityCode) {
            return emWamp.call("/user/deposit#prepare",
            {
                paymentMethodCode: _supportedPaymentMethodCodes.VISA,
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
                paymentMethodCode: _supportedPaymentMethodCodes.Maestro,
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
                paymentMethodCode: _supportedPaymentMethodCodes.MasterCard,
                fields: {
                    gamingAccountID: gamingAccountId,
                    currency: currency,
                    amount: amount,
                    payCardID: payCardId,
                    cardSecurityCode: cardSecurityCode
                }
            });
        };

        //
        // Confirm and process the prepared deposit transaction. this method is only called for the prepared transaction whose status is "setup"
        //
        // Return
        //
        //  {
        //      "pid": "7e140c1ea6f24e53890df2dc3d213366",
        //      "status": "redirection",
        //      "redirectionForm": "<form id=\"deposit-form\" name=\"deposit-form\" action=\"https://secure.metacharge.com/mcpe/acs\" method=\"post\" target=\"deposit-3rd-iframe\"> <input type=\"hidden\" name=\"PaReq\" value=\"VGVzdE1vZGVUcmFuc2FjdGlvblRlc3RNb2RlVHJhbnNhY3Rpb25UZXN0TW9kZVRyYW5zYWN0aW9uVGVzdE1vZGVUcmFuc2FjdGlvbn5+fn5+fn5+fn5+fn5+fn4=\" /> <input type=\"hidden\" name=\"MD\" value=\"MTUzMTYzNDcxMTUzMTYzNDcxMTUzMTYzNDcxMTUzMTYzNDcxMTUzMTYzNDcx\" /> <input type=\"hidden\" name=\"TermUrl\" value=\"http://localhost:9644/Payment/Postback?_em_dm=www.thrills.com&_em_pid=7e140c1ea6f24e53890df2dc3d213366&_em_sid=3VIZIFGXVWA8&gm_sid=2bd6cdf52cb248cf864c6cb626b6b03f&gm_hc=b1c9143044592cc08b11b785ff5ac1a0d6546ae7&ownw=0\" /> </form>"
        //  }
        //
        _service.confirm = function (pid) {
            return emWamp.call("/user/deposit#confirm", { pid: pid });
        };

        //
        // Query the information for successfully completed deposit transaction
        //
        // Return
        //
        //  {
        //      "status": "success",
        //      "transactionID": 1648350,
        //      "time": "2013-10-06T09:09:48.327",
        //      "credit": {
        //          "currency": "CNY",
        //          "amount": 100,
        //          "name": "Casino"
        //      },
        //      "debit": {
        //          "currency": "EUR",
        //          "amount": 13.02,
        //          "name": "631234...2345"
        //      },
        //      "fees": [ {
        //          "currency": "CNY",
        //          "amount": 8.31
        //      }]
        //  }
        // 
        // status  [string] Indicate the status.
        //  error : the transaction is failed
        //  incomplete : the transaction is incomplete
        //  success : the transaction is completed successfully
        //
        // desc  [string] the description for the error, only appears when status = error
        _service.getTransactionInfo = function(pid) {
            return emWamp.call("/user/deposit#getTransactionInfo", { pid: pid });
        };

        _service.sendReceiptEmail = function(pid, receiptHtml) {
            return emWamp.call("/user/deposit#sendReceiptEmail",
            {
                pid:pid,
                receiptHtml: receiptHtml
            });
        };

        // #endregion Deposit
        
        return _service;
    };

})();