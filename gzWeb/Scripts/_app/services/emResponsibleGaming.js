(function() {
    "use strict";

    APP.factory("emResponsibleGaming", ["$q", "emWamp", serviceFunc]);

    function serviceFunc($q, emWamp) {

        var _service = {};

        //
        // Get the cool-off config for current logged-in user.
        //
        // Parameters
        //  None
        //
        // Return
        //  {
        //      "reasons": [
        //          {
        //              "name": "restrict-playing",
        //              "text": "I want to restrict my playing"
        //          },
        //          {
        //              "name": "unsatisfied",
        //              "text": "I am unsatisfied with the website"
        //          },
        //          {
        //              "name": "other",
        //              "text": "Other"
        //          },
        //          ...
        //      ],
        //      "unsatisfiedReasons": [
        //          {
        //              "name": "customer-service",
        //              "text": "Customer service"
        //          },
        //          {
        //              "name": "other",
        //              "text": "Other"
        //          },
        //          ...
        //      ],
        //      "periods": [
        //          {
        //              "name": "CoolOffFor24Hours",
        //              "text": "24 hours",
        //              "dateSelector": false
        //          },
        //          {
        //              "name": "CoolOffUntilSelectedDate",
        //              "text": "until",
        //              "dateSelector": true,
        //              "maximumDate": "2015-12-26"
        //          },
        //          ...
        //      ]
        //  };
        //
        // reasons [JSON object array], Represents the cool off reasons can choose.
        //  name [string]: name of the reason
        //  text [string]: localized text of the reason
        // If 'unsatisfied' is chosen, the unsatisfied reason must be set
        // If 'other' is chosen, user need to enter the reason.
        //
        // unsatisfiedReasons [JSON object array], Represents the unsatisfied reasons user can choose
        //  name [string] : name of the period
        //  text [string] :  localized text of the period
        // If 'other' is chosen, user need to enter the reason.
        //
        // periods [JSON object array], Represents the periods user can set
        //  name [string] : name of the period
        //  text [string] :  localized text of the period
        //  dateSelector [boolean] : Indicates if the period allow the date selector
        //  maximumDate [string] : The maximum date of the date selector, only exists when "dateSelector" is true.
        //
        _service.coolOffGetCfg = function () {
            return emWamp.call("/user/coolOff#getCfg");
        };

        //
        // Enable cool-off for current logged-in user.
        //
        // Parameter
        //  var parameters =
        //  {
        //      "reason": "", //string
        //      "unsatisfiedReason": "", //string
        //      "period": "" //string
        //  };
        //
        // reason [string]
        // Indicates the reason to close the account.
        // 
        // unsatisfiedReason [string]
        // Indicates the unsatisfied reason to close the account, only exist when the reason is unsatisfied.
        //
        // period [string]
        // Indicates the period to close the account.
        // Must be one of the periods returned from /user/coolOff#getCfg
        //
        // Return
        // None, the connection will be dropped immediately before the method returns.
        //
        // Remarks
        // Cool-Off means that the account will remain closed for a period indicated by the parameter, 
        //  and will not be reactivated under any circumstances during the cool-off period.
        //
        _service.coolOffEnable = function (reason, unsatisfiedReason, period) {
            return emWamp.call("/user/coolOff#enable",
            {
                reason: reason,
                unsatisfiedReason: unsatisfiedReason,
                period: period
            });
        };

        //
        // Get the self-exclusion config for current logged-in user.
        //
        // Parameters
        //  None
        //
        // Return
        //  {
        //      "periods": [
        //          {
        //              "name": "SelfExclusionFor6Months",
        //              "text": "6 months",
        //              "dateSelector": false
        //          },
        //          {
        //              "name": "SelfExclusionFor1Year",
        //              "text": "1 year",
        //              "dateSelector": false
        //          },
        //          {
        //              "name": "SelfExclusionFor5Years",
        //              "text": "5 years",
        //              "dateSelector": false
        //          },
        //          {
        //              "name": "SelfExclusionUntilSelectedDate",
        //              "text": "until",
        //              "dateSelector": true,
        //              "minimumDate": "2015-12-26"
        //          },
        //          {
        //              "name": "SelfExclusionPermanent",
        //              "text": "Permanent",
        //              "dateSelector": false
        //          }
        //      ]
        //  };
        //
        // periods [JSON object array], Represents the periods user can set
        //  name [string] : name of the period
        //  text [string] :  localized text of the period
        //  dateSelector [boolean] : Indicates if the period allow the date selector
        //  minimumDate [string] : The minimum date of the date selector, only exists when "dateSelector" is true.
        //
        _service.selfExclusionGetCfg = function () {
            return emWamp.call("/user/selfExclusion#getCfg");
        };

        //
        // Enable self-exclusion for current logged-in user.
        //
        // Parameter
        //
        //  var parameters = {
        //      "period": ""   //string
        //  };
        //
        // period [string]
        // Indicates the period to close the account.
        // Must be one of the periods returned from /user/selfExclusion#getCfg
        //
        // Return
        // None, the connection will be dropped immediately before the method returns.
        //
        // Remarks
        // Self-exclusion means that the account will remain closed for a period indicated by the parameter, 
        // and will not be reactivated under any circumstances during the exclusion period. 
        // Front-end may hide the 'permanent' option and ask the end-user to send an email to support if they do want to close the account permanently.
        //
        // Front-end implementation may hide certain option (i.e. hide "Permanent" option) as needed.
        //
        _service.selfExclusionEnable = function (period) {
            return emWamp.call("/user/selfExclusion#enable", { period: period });
        };

        // #region Limits (https://help.gammatrix-dev.net/help/Limits.html)

        //
        // Get the limit rules for current logged-in user. 
        //
        // https://help.gammatrix-dev.net/help/getLimits.html
        //
        _service.getLimits = function () {
            return emWamp.call("/user/limit#getLimits");
        };

        //
        // Set deposit limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/setDepositLimit.html
        //
        _service.setDepositLimit = function (period, amount, currency) {
            return emWamp.call("/user/limit#setDepositLimit", {
                period: period,
                amount: amount,
                currency: currency
            });
        };

        //
        // Remove deposit limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/removeDepositLimit.html
        //
        _service.removeDepositLimit = function () {
            return emWamp.call("/user/limit#removeDepositLimit");
        };

        //
        // Set wagering limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/setWageringLimit.html
        //
        _service.setWageringLimit = function (period, amount, currency) {
            return emWamp.call("/user/limit#setWageringLimit", {
                period: period,
                amount: amount,
                currency: currency
            });
        };

        //
        // Remove wagering limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/removeWageringLimit.html
        //
        _service.removeWageringLimit = function () {
            return emWamp.call("/user/limit#removeWageringLimit");
        };

        //
        // Set loss limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/setLossLimit.html
        //
        _service.setLossLimit = function (period, amount, currency) {
            return emWamp.call("/user/limit#setLossLimit",
            {
                period: period,
                amount: amount,
                currency: currency
            });
        };

        //
        // Remove loss limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/removeLossLimit.html
        //
        _service.removeLossLimit = function () {
            return emWamp.call("/user/limit#removeLossLimit");
        };

        //
        // Set session limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/setSessionLimit.html
        //
        _service.setSessionLimit = function (minutes) {
            return emWamp.call("/user/limit#setSessionLimit", { minutes: minutes });
        };
        
        //
        // Remove session limit for current logged-in user. For more information, please click (https://help.gammatrix-dev.net/help/Limits.html).
        //
        // https://help.gammatrix-dev.net/help/removeSessionLimit.html
        //
        _service.removeSessionLimit = function () {
            return emWamp.call("/user/limit#removeSessionLimit");
        };
        
        // #endregion Limits
        
        // #region RealityCheck
        
        //
        // This is to allow set and get reality check values for players who are flagged with UK residency.
        // There will show a pop-up to notify the player when the time set in the reality check has passed while the player gambles on a casino game
        //
        // https://help.gammatrix-dev.net/help/RealityCheck.html
        //

        //
        // This is to get the available reality check values, and user can select from one of returned available reality check values only.
        //
        // https://help.gammatrix-dev.net/help/getCfg2.html
        // 
        _service.realityCheckGetCfg = function () {
            return emWamp.call("/user/realityCheck#getCfg");
        };

        //
        // This is to set reality check value for a specific user.
        //
        // https://help.gammatrix-dev.net/help/set.html
        // 
        _service.realityCheckSet = function (value) {
            return emWamp.call("/user/realityCheck#set", { value: value });
        };

        //
        // This is to get the reality check value set earlier by a specific user.
        //
        // https://help.gammatrix-dev.net/help/get.html
        // 
        _service.realityCheckGet = function () {
            return emWamp.call("/user/realityCheck#get");
        };

        // #endregion

        return _service;
    };

})();