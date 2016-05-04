﻿(function() {
    "use strict";

    APP.factory("emBankingWithdraw", ["$q", "emWamp", serviceFunc]);
    function serviceFunc($q, emWamp) {

        // ---------------------------------------------------------------------------------------------------
        //
        // Withdraw Flow (https://help.gammatrix-dev.net/help/EveryMatrixWebClientAPIHelp.html?Withdraw.html)
        //
        // ---------------------------------------------------------------------------------------------------

        var _service = {};

        _service.getPaymentMethods = function(currency, includeAll) {
            return emWamp.call("/user/withdraw#getPaymentMethods",
            {
                currency: currency,
                includeAll: includeAll
            });
        };

        _service.getPaymentMethodCfg = function(paymentMethodCode, payCardId) {
            return emWamp.call("/user/withdraw#getPaymentMethodCfg",
            {
                paymentMethodCode: paymentMethodCode,
                payCardId: payCardId
            });
        };

        _service.prepare = function (paymentMethodCode, fields) {
            return emWamp.call("/user/withdraw#prepare",
            {
                paymentMethodCode: paymentMethodCode,
                fields: fields
            });
        };

        _service.confirm = function (pid) {
            return emWamp.call("/user/withdraw#confirm", { pid: pid });
        };

        _service.getTransactionInfo = function (pid) {
            return emWamp.call("/user/withdraw#getTransactionInfo", { pid: pid });
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
        _service.registerPayCard = function (parameter) {
            return emWamp.call("/user/withdraw#registerPayCard", parameter);
        };

        _service.getPendingWithdrawals = function () {
            return emWamp.call("/user/withdraw#getPendingWithdrawals");
        };

        _service.rollback = function (id) {
            return emWamp.call("/user/withdraw#rollback", { id: id });
        };

        return _service;
    };

})();