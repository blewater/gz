(function() {
    "use strict";

    APP.factory("emWamp", ['$rootScope', '$log', 'constants', 'localStorageService', 'emConCfg', 'message', '$route', 'vcRecaptchaService', '$q', emWampService]);
    function emWampService($rootScope, $log, constants, localStorageService, emConCfg, message, $route, vcRecaptchaService, $q) {

        var WEBSOCKET_API_URL = emConCfg.webSocketApiUrl;
        var LONGPOLL_API_URL = emConCfg.fallbackApiUrl;
        var DOMAIN_PREFIX = emConCfg.domainPrefix;

        function stripSensitiveData(parameters) {
            angular.forEach(parameters, function (value, key) {
                if (key.indexOf("password") !== -1) {
                    parameters[key] = "******";
                }
                //else if (key.indexOf("iovationBlackbox") !== -1) {
                //    parameters[key] = value.substr(0, 8) + " ....";
                //}
            });
            return parameters;
        }

        function getLogMessage(uri, parameters) {
            var traceMsg = "emWamp: '" + uri + "'. ";
            if (angular.isDefined(parameters)) {
                traceMsg = traceMsg + " with parameters: '" + angular.toJson(stripSensitiveData(parameters)) + "'. ";
            }
            return traceMsg;
        }


        // Represents if fallback is required
        var enableFallback = false;

        (function () {
            // First, it checks the WebSocket support in web browser. If not,  fallback will be enabled.
            if (!("WebSocket" in window))
                enableFallback = true;

            if( !enableFallback ){
                // Second, websocket connection could be blocked by proxy(http://www.infoq.com/articles/Web-Sockets-Proxy-Servers) or other intermediate nodes
                // If previous websocket connection attempt failed, should enable fallback now
                // The previous status is stored in localStorage, according to http://caniuse.com/#search=localstorage & http://caniuse.com/#search=websocket
                // If websocket is supported, localStorage must be supported.
                var lastFailureTime = localStorageService.get(constants.storageKeys.lastFailureTime);
                if (!isNaN(lastFailureTime)) {
                    var offset = (new Date()).getTime() - lastFailureTime;
                    // If the last failure happened within recent 1 hour, enable fallback
                    enableFallback = offset < 3600000 && offset >= 0;
                }
            }
        })();

        var SessionStatus = {
            NOT_CONNECTED: "not connected",
            CONNECTING: "connecting",
            CONNECTED: "connected",
            RESUMING_LOGIN: "resuming login",
            INIT_COMPLETED: "initialization completed",
            LOGGED_IN: "login",
            LOGGED_OUT: "logout"
        };

        var SessionMgr = {
            _connection: null, // the connection
            _session: null, // the session
            _status: SessionStatus.NOT_CONNECTED,

            notifyStatusChange: function (status, data) {
                data = data || {};
                if (status != SessionStatus.RESUMING_LOGIN && status != SessionStatus.INIT_COMPLETED) {
                    SessionMgr._status = status;
                    if (status == SessionStatus.LOGGED_OUT)
                        SessionMgr._status = SessionStatus.CONNECTED;
                    data.status = SessionMgr._status;
                }
                else
                    data.status = status;

                $rootScope.$broadcast(constants.events.SESSION_STATE_CHANGE, data);

                //SessionMgr.statusChangeHandlers.publish({
                //    status: status,
                //    code: data.code,
                //    reason: data.reason
                //});
            },

            isLoggedIn: function () {
                return SessionMgr._status == SessionStatus.LOGGED_IN;
            },

            // establish the connection
            init: function (callback) {
                if (typeof (WEBSOCKET_API_URL) != typeof (''))
                    throw '[WEBSOCKET_API_URL] is not defined!';

                if (typeof (LONGPOLL_API_URL) != typeof (''))
                    throw '[LONGPOLL_API_URL] is not defined!';

                if (typeof (DOMAIN_PREFIX) != typeof (''))
                    throw '[DOMAIN_PREFIX] is not defined!';

                var timer = null; // timer of websocket connection establish
                SessionMgr.notifyStatusChange(SessionStatus.CONNECTING);

                var onchallenge = function (n, method, extra) {
                    return loadQuotaCaptcha(method).then(onQuotaCaptchaTestPassed);
                    //return autobahn.when.promise(function (resolve, reject) {
                    //    if (timer != null) {
                    //        clearTimeout(timer);
                    //        timer = null;
                    //    }

                    //    //[4, "GoogleRecaptchaV2", {"key":"6LcoFCkTAAAAAMSiR6yUa8LZC32qDHlHSywwHxIz"}]
                    //    var success = loadQuotaCaptcha(method, extra.key, function (captchaResponse) {
                    //        resolve(captchaResponse);
                    //    });
                    //    if (!success) {
                    //        reject(`Unknown challenge type '${method}'`);
                    //    }
                    //});
                };

                if (enableFallback) {
                    SessionMgr._connection = new autobahn.Connection({
                        transports: [
                            {
                                'type': 'longpoll',
                                'url': LONGPOLL_API_URL,
                                'request_timeout': 30000
                            }
                        ],
                        realm: DOMAIN_PREFIX,
                        onchallenge: onchallenge
                    });
                } else {
                    var storedClientId = localStorageService.get(constants.storageKeys.clientId);
                    var cid = "";
                    if (storedClientId != null && storedClientId != "")
                        cid = encodeURIComponent(storedClientId);
                    SessionMgr._connection = new autobahn.Connection({
                        transports: [
                           {
                               'type': 'websocket',
                               'url': WEBSOCKET_API_URL + '?cid=' + cid,
                               'max_retries': 3
                           }
                        ],
                        realm: DOMAIN_PREFIX,
                        onchallenge: onchallenge
                    });
                }

                // If websocket connection is being established, detect its failure
                if (!enableFallback) {
                    // It is not safe to detect the websocket connection failure via callback 
                    // Since some intermediate node may prevent the websocket handshake and no callback will be fired.
                    // here take use a timer and if the connection can not be established within 7 seconds, then re-try in fallback
                    timer = setTimeout(function () {
                        localStorageService.set(constants.storageKeys.lastFailureTime, (new Date()).getTime()); // save the last fail time
                        self.location = self.location; // refresh current page and it will re-try with fallback
                    }, 7000);
                }

                SessionMgr._connection.onopen = function (session) {
                    if (timer != null) {
                        clearTimeout(timer);
                        timer = null;
                    }

                    SessionMgr._session = session;
                    SessionMgr.notifyStatusChange(SessionStatus.CONNECTED);

                    SessionMgr.call("/user#getSessionInfo").then(
                        function (json) {
                            if (json.isAuthenticated)
                                SessionMgr.notifyStatusChange(SessionStatus.LOGGED_IN);
                        }, function (err) {
                        }
                    );

                    SessionMgr.call("/connection#getClientIdentity").then(
                        function (json) {
                            localStorageService.set(constants.storageKeys.clientId, json.cid);
                        }, function (err) {
                        }
                    );

                    SessionMgr.subscribe("/sessionStateChange", function (kwargs, args, details) {
                        kwargs.initialized = $rootScope.initialized;
                        $log.trace("SESSION_STATE_CHANGE => args: " + angular.toJson(args) + ", kwargs: " + angular.toJson(kwargs) + ", details: " + angular.toJson(details));

                        if (kwargs.code == 0) { // 0 means this user is logged-in
                            SessionMgr.notifyStatusChange(SessionStatus.LOGGED_IN, kwargs);
                        } else {
                            SessionMgr.notifyStatusChange(SessionStatus.LOGGED_OUT, kwargs);
                        }
                    }).then(
                        // after the event is subscribed, set the group id
                        function (subscription) {
                            SessionMgr.notifyStatusChange(SessionStatus.INIT_COMPLETED);
                        },
                        function (errorCode) {
                            SessionMgr.notifyStatusChange(SessionStatus.NOT_CONNECTED);
                        }
                    );

                    //SessionMgr._session.subscribe("/account/balanceChanged", function (kwargs, args, details) {
                    //    $rootScope.$broadcast(constants.events.ACCOUNT_BALANCE_CHANGED, kwargs);
                    //    $log.debug("ACCOUNT_BALANCE_CHANGED => args: " + angular.toJson(args) + ", kwargs: " + angular.toJson(kwargs) + ", details: " + angular.toJson(details));
                    //});
                    SessionMgr._session.subscribe("/deposit/statusChanged", function (kwargs, args, details) {
                        $rootScope.$broadcast(constants.events.DEPOSIT_STATUS_CHANGED, kwargs);
                        $log.debug("DEPOSIT_STATUS_CHANGED => args: " + angular.toJson(args) + ", kwargs: " + angular.toJson(kwargs) + ", details: " + angular.toJson(details));
                    });
                    SessionMgr._session.subscribe("/withdraw/statusChanged", function (kwargs, args, details) {
                        $rootScope.$broadcast(constants.events.WITHDRAW_STATUS_CHANGED, kwargs);
                        $log.debug("WITHDRAW_STATUS_CHANGED => args: " + angular.toJson(args) + ", kwargs: " + angular.toJson(kwargs) + ", details: " + angular.toJson(details));
                    });

                    onOpen();

                    if (angular.isFunction(callback))
                        callback();
                };

                SessionMgr._connection.onclose = function (status, data) {
                    SessionMgr.notifyStatusChange(SessionStatus.NOT_CONNECTED);
                };

                SessionMgr._connection.open();
            },

            pendingCalls: [],

            // Wrap the native "call" method from autobahn.js.
            // 1. Add a common parameter "language" which is loaded from cookie
            // 2. convert the input / output type for backward compatibility of WAMP v1.0
            call: function () {
                if (SessionMgr._session != null) {
                    if (arguments.length == 0)
                        throw new Error("Not enough parameter");
                    var procUri = arguments[0];
                    var parameters = {};
                    if (arguments.length > 1)
                        parameters = arguments[1];

                    //var lang = CookieHelper.get('language');
                    //if (lang != null && lang.length > 0) {
                    //    parameters.culture = lang;
                    //}

                    var callReturn = SessionMgr._session.call.apply(SessionMgr._session, [procUri, [], parameters]);
                    //var callReturn = SessionMgr._session.id
                    //    ? SessionMgr._session.call.apply(SessionMgr._session, [procUri, [], parameters])
                    //    : SessionMgr.init(function () {
                    //          return SessionMgr._session.call.apply(SessionMgr._session, [procUri, [], parameters])
                    //      });


                        
                    var originalArguments = arguments;
                    // hook
                    var orginalFunc = callReturn.then;
                    callReturn.then = function (successCallback, failureCallback) {
                        function _success(d) {
                            $log.trace(getLogMessage(procUri, parameters) + "Success.");

                            if (typeof (successCallback) === 'function')
                                successCallback(d && d.kwargs);
                        }

                        function _error(e) {
                            $log.error((procUri, parameters) + "Failed with error: '" + angular.toJson(e) + "'.");

                            // show popup
                            if (e.error == 'wamp.quota_exhausted') {
                                // cache the calls to be sent later
                                if (procUri.indexOf('/connection#') < 0) {
                                    SessionMgr.pendingCalls.push({
                                        arguments: originalArguments,
                                        successCallback: successCallback,
                                        failureCallback: failureCallback
                                    });
                                }
                                else if (typeof (failureCallback) === 'function')
                                    failureCallback(e.kwargs);

                                loadQuotaCaptcha(e.kwargs.type).then(onQuotaCaptchaTestPassed);
                            }
                            else if (typeof (failureCallback) === 'function')
                                failureCallback(e.kwargs);
                        }
                        return orginalFunc.call(callReturn, _success, _error);
                    };

                    return callReturn;
                }
            },

            // Wrap the "subscribe" method from autobahn.js.
            // Convert the callback parameter for backward compatibility of WAMP v1.0
            subscribe: function (topicUri, callback) {
                return SessionMgr._session.subscribe(topicUri, function (args, kwargs, details) {
                    if (typeof (callback) === 'function')
                        callback(kwargs, args, details);
                });
            },

            // Wrap the "register" method from autobahn.js.
            register: function (procUri, callback) {
                return SessionMgr._session.register(procUri, function (args, kwargs, details) {
                    if (typeof (callback) === 'function')
                        callback(procUri, kwargs);
                });
            },

            // Wrap the "register" method from autobahn.js.
            unregister: function (procUri, callback) {
                return SessionMgr._session.unregister(procUri, function () {
                    if (typeof (callback) === 'function')
                        callback(procUri);
                });
            }
        };

        var challengeIsOpen = false;
        function loadQuotaCaptcha(type) {
            var preloader = document.getElementById("preloader");
            if (preloader) {
                var body = document.getElementsByTagName("BODY")[0];
                preloader.className = "die";
                setTimeout(function () {
                    body.removeChild(preloader);
                }, 1000);

                var bg = document.createElement("img")
                bg.setAttribute("id", "bg");
                bg.className = "bg";
                bg.src = "../../Content/Images/casino-default.png";
                body.appendChild(bg);
            }

            var deferred = $q.defer();
            var promise = deferred.promise;
            if (!challengeIsOpen) {
                if (type === 'GoogleRecaptchaV2') {
                    var renderedChallengePromise = message.open({
                        nsType: 'modal',
                        nsSize: 'sm',
                        nsTemplate: '_app/everymatrix/challenge.html',
                        nsCtrl: 'challengeCtrl',
                        nsStatic: true,
                        nsShowClose: false
                    });
                    challengeIsOpen = true;
                    renderedChallengePromise.then(function (renderedChallengeResponse) {
                        SessionMgr.call("/connection#increaseRequestQuota", { challengeResponse: renderedChallengeResponse }).then(
                            function (json) {
                                vcRecaptchaService.reload();
                                if (json.success) {
                                    challengeIsOpen = false;

                                    var bg = document.getElementById("bg");
                                    bg.style.display = 'none';
                                    onOpen();

                                    // resent the queued requests
                                    for (; ;) {
                                        var first = SessionMgr.pendingCalls.shift();
                                        if (first)
                                            SessionMgr.call.apply(this, first.arguments).then(first.successCallback, first.failureCallback);
                                        else
                                            break;
                                    }
                                }
                                else {
                                    alert('Recaptcha test failed, please try again');
                                    vcRecaptchaService.reload();
                                }
                            }
                            , function (err) {
                                challengeIsOpen = false;
                                $log.error(err);
                                vcRecaptchaService.reload();
                            }
                        );
                    }, function (renderedChallengeError) {
                        vcRecaptchaService.reload();
                        $log.error(renderedChallengeError);
                        challengeIsOpen = false;
                        deferred.reject(renderedChallengeError);
                    })
                }
                else {
                    var err = 'Unknown challenge type: ' + type;
                    $log.error(err);
                    deferred.reject(err);
                }
            }
            return promise;
        }

        var _call = SessionMgr.call;

        var service = {
            call: _call,
            
            // #region Account
            validateUsername: function(username) {
                return _call("/user/account#validateUsername", { username: username });
            },

            validateEmail: function (email) {
                return _call("/user/account#validateEmail", { email: email });
            },

            register: function(parameters) {
                return _call("/user/account#register", parameters);
            },

            //
            // Get the country list to populate the registration form or profile form.
            //
            // Parameter
            //  {
            //      expectRegions: true,
            //      filterByCountry: '',
            //      excludeDenyRegistrationCountry: true
            //  }
            //
            // 
            // expectRegions: [boolean, optional]
            // Indicates if the region list should be included in the response for each country.  
            // 
            // filterByCountry: [string, optional]
            // Filter the country by country code (ISO3166 Alpha2).  
            //
            // excludeDenyRegistrationCountry: [boolean, optional]
            // Indicate if the response should exclude the countries which deny gambling.
            //
            getCountries: function (expectRegions, filterByCountry, excludeDenyRegistrationCountry) {
                return _call("/user/account#getCountries", {
                    expectRegions: expectRegions,
                    filterByCountry: filterByCountry,
                    excludeDenyRegistrationCountry: excludeDenyRegistrationCountry
                });
            },

            getCurrencies: function () {
                return _call("/user/account#getCurrencies");
            },

            getProfile: function () {
                return _call("/user/account#getProfile");
            },

            updateProfile: function (parameters) {
                return _call("/user/account#updateProfile", parameters);
            },

            getApplicableBonuses: function (parameters) {
                return _call("/user#getApplicableBonuses", parameters);
            },

            applyBonus: function (parameters) {
                return _call("/user/bonus#apply", parameters);
            },

            getGrantedBonuses: function () {
                return _call("/user/bonus#getGrantedBonuses", {});
            },

            forfeit: function (bonusID) {
                return _call("/user/bonus#forfeit", { bonusID: bonusID });
            },

            moveToTop: function (bonusID) {
                return _call("/user/bonus#moveToTop", { bonusID: bonusID });
            },

            //
            // This is the first step for logged-in user to change email. Click (https://help.gammatrix-dev.net/help/Email.html) to view the flow explanation.
            //
            // https://help.gammatrix-dev.net/help/sendVerificationEmailToNewMailbo.html
            //
            // Remarks
            // Initially, end-user is not asked for captcha test when changing email. However, if end-user made too many failed attempts with incorrect password, 
            // they have to pass the captcha test. If captcha test is required but not all the 3 parameters (captchaPublicKey / captchaChallenge / captchaResponse) 
            // are indicated in the request, this method will return corresponding value.
            //
            // The suggested front-end logic is:
            // After the HTML page is loaded, captcha is not loaded by default. Call this method with the parameters, certainly captchaPublicKey / captchaChallenge / captchaResponse 
            // are default to null when captcha is not loaded.
            // If the return value indicates that the captcha test is required, front-end loads the captcha and let the end-user try again.
            // 
            // If the return value is true, two emails are sent out.
            // One email is sent to the original mailbox, to inform that the email is being changed.
            // The other email is sent to the new mailbox for verification. 
            //
            sendVerificationEmailToNewMailbox: function (email, password, emailVerificationURL, captchaPublicKey, captchaChallenge, captchaResponse) {
                return _call("/user/email#sendVerificationEmailToNewMailbox", {
                    email: email,
                    password: password,
                    emailVerificationURL: emailVerificationURL,
                    captchaPublicKey: captchaPublicKey,
                    captchaChallenge: captchaChallenge,
                    captchaResponse: captchaResponse
                });
            },

            //
            // This is the 2nd step for change email procedure. Click (https://help.gammatrix-dev.net/help/Email.html) to view the flow explanation.
            //
            // https://help.gammatrix-dev.net/help/verifyNewEmail.html
            //
            verifyNewEmail: function(key, email) {
                return _call("/user/email#verifyNewEmail", { key: key, email: email });
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
                JL("emWamp").info("login attempt for user: " + parameters.usernameOrEmail);
                return _call("/user#login", parameters);
            },

            acceptTC: function() {
                return _call("/user#acceptTC");
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

            logout: function () {
                _call("/user#logout");
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
            // Get the password policy for client-side validation.
            //
            // Return
            //
            //  {
            //      "regularExpression": "(?=.*\\d+)(?=.*[A-Za-z]+).{8,20}",
            //      "message": "Password must contain at least1 letter and 1 digit, and its minimal length is 8."
            //  }
            //
            // regularExpression [string]
            // The regular expression configured in back-end.
            // 
            // message [string]
            // The message for the password requirement.
            //
            getPasswordPolicy: function() {
                return _call("/user/pwd#getPolicy");
            },

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

            // #region Balance
            watchBalance : function (account, callback) {
                return SessionMgr._session.call("/user/account#watchBalance").then(function (result) {
                    SessionMgr.subscribe("/account/balanceChanged", function (data) {
                        callback(data);
                    });
                });
            },
            unwatchBalance: function (account) {
                return SessionMgr._session.call("/user/account#unwatchBalance");
            },
            // #endregion

            // #region Init
            init: SessionMgr.init
            // #endregion
        };

        function onOpen(event, info) {
            SessionMgr._session.call('/connection#getClientIdentity').then(function (result) {
                localStorageService.set(constants.storageKeys.clientId, result.kwargs.cid);
            }, function (error) {
                $log.error(error.kwargs.desc);
            });
            $rootScope.$broadcast(constants.events.CONNECTION_INITIATED);
        }

        return service;
    };

})();