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

        var _onSessionStateChange = function(args, kwargs, details) {
            console.log(
                "sessionStateChange => args: " + angular.toJson(args) +
                ", kwargs: " + angular.toJson(kwargs) +
                ", details: " + angular.toJson(details));
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
            login: function(parameters) {
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
                return _call("/user#logout");
            },

            // #endregion

            // #region Password

            //
            // Change Password & Reset Password (triggered by end-user)
            // 
            // Logged-in users are allowed to change their password via /user/pwd#change method.
            //
            // And if end-users forget their passwords, they can reset their passwords without login by the following steps
            // 1. Call /user/pwd#sendResetPwdEmail method to send an reset-password to their registered mailboxes.
            // 2. Clicking the link in the reset-password email, redirect to modify-password page.
            // 3. Filling the new password and call /user/pwd#reset to change the password.
            // 
            // The /user/pwd#sendResetPwdEmail method accepts a mandatory parameter named "changePwdURL", 
            // which will be inserted into the reset-password email as the link url.
            //
            // The "changePwdURL" is appended with a generated key for subsequent methods.
            //
            //For example:
            //
            // changePwdURL : https://www.some-site.com/reset-pwd/
            // Url in email : https://www.some-site.com/reset-pwd/ab9f7861-6268-4469-8496-4d95e48228b8
            //
            // changePwdURL : https://www.some-site.com/reset-pwd?key=
            // Url in email : https://www.some-site.com/reset-pwd?key=ab9f7861-6268-4469-8496-4d95e48228b8
            //
            // End-user clicks the link in the received email, and redirected to the reset-pwd page.
            // In the page, it first calles /user/pwd#isResetPwdKeyAvailable method to verify the key from url query string. 
            // If the key is valid, a form is presented with a password input.
            //
            // End-user fills the new password and click submit, then /user/pwd#reset method is called with the key & the new password.
            //

            //
            // Change the password of the current logged-in user.
            //
            // Parameters
            //
            //  {
            //      "oldPassword": "",
            //      "newPassword": "",
            //      "captchaPublicKey": "",
            //      "captchaChallenge": "",
            //      "captchaResponse": ""
            //  }
            //
            // oldPassword [string, mandatory] 
            // The current password of the logged-in user.
            //
            // newPassword [string, mandatory] 
            // The new password to be changed to. Password has to comply with the password policy configured in back-end 
            // which can be retrieved from /user/pwd#getPolicy method.
            // 
            // captchaPublicKey [string, mandatory when captcha test is required]
            // Google reCAPTCHA public key, click here for more details.
            // 
            // captchaChallenge [string, mandatory when captcha test is required]
            // Google reCAPTCHA challenge, click here for more details.
            // 
            // captchaResponse [string, mandatory when captcha test is required]
            // Google reCAPTCHA response, click here for more details.
            //
            // Return
            //
            //  {
            //      "isCaptchaEnabled": true
            //  }
            // 
            // isCaptchaEnabled [boolean]
            // Represents if CAPTCHA test is enabled.  If the value is true, the next request of this call 
            // should be emitted with captchaPublicKey / captchaChallenge / captchaResponse parameters.
            //
            changePassword: function(parameters) {
                return _call("/user/pwd#change", parameters);
            },

            //
            // This is the first step of forgot-password flow. Check the flow explanation please.
            //
            // Parameter
            //
            // {
            //      "email": "",
            //      "changePwdURL": "",
            //      "captchaPublicKey": "",
            //      "captchaChallenge": "",
            //      "captchaResponse": ""
            // }
            //
            // email [string, mandatory]
            // The registered email address.
            //
            // changePwdURL [string, mandatory]
            // The url of the link inserted in the verification email.
            //
            // captchaPublicKey [string, mandatory when captcha test is required]
            // Google reCAPTCHA public key, click here for more details.
        // 
            // captchaChallenge [string, mandatory when captcha test is required]
            // Google reCAPTCHA challenge, click here for more details.
            // 
            // captchaResponse [string, mandatory when captcha test is required]
            // Google reCAPTCHA response, click here for more details.
            //
            sendResetPwdEmail: function (parameters) {
                return _call("/user/pwd#sendResetPwdEmail", parameters);
            },

            //
            // This is an auxiliary method for forgot-password flow. Check the flow explanation please.
            //
            // key [string, mandatory]
            // The key to be checked.
            //
            // Returns a boolean value indicating if the key is valid.
            //
            isResetPwdKeyAvailable: function(key) {
                return _call("/user/pwd#isResetPwdKeyAvailable", { key: key });
            },

            //
            // This is the last step of forgot-password flow. Check the flow explanation please.
            //
            // key [string, mandatory]
            // The key.
            //
            // password [string, mandatory]
            // The password to be set. Password has to comply with the password policy configured in back-end 
            // which can be retrieved from /user/pwd#getPolicy method.
            resetPassword: function(key, password) {
                return _call("/user/pwd#reset", { key: key, password: password });
            },

            // #endregion
        };

        $wamp.subscribe("/sessionStateChange", _onSessionStateChange);
        $wamp.open();

        return service;
    };

})();