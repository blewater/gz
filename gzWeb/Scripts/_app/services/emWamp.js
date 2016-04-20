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
                return _call("/user#logout");
            }

            // #endregion
        // 
        };

        $wamp.open();

        return service;
    };

})();