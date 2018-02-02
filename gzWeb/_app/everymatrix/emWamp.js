(function() {
    "use strict";

    APP.factory("emWamp", ['$wamp', '$rootScope', '$log', 'constants', 'localStorageService', 'emConCfg', 'message', '$route', 'vcRecaptchaService', '$q', emWampFunction]);
    function emWampFunction($wamp, $rootScope, $log, constants, localStorageService, emConCfg, message, $route, vcRecaptchaService, $q) {

        var _logError = function(error) {
            $log.error(error);
            if (error.indexOf('Invalid frame header') !== -1) {
                // Your session has been idle for more than 90 minutes and you have been logged out for security reasons.
                logout();
            }
        };

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

        var apiQuotaExceeded = false;
        var _call = function (uri, parameters) {
            if (apiQuotaExceeded === true && uri !== '/connection#increaseRequestQuota')
                return $q.defer().promise;

            var callReturn = $wamp.call(uri, [], parameters);
            var originalFunc = callReturn.then;
            callReturn.then = function(successCallback, failureCallback) {
                function success(d) {
                    $log.trace(getLogMessage(uri,parameters) + "Success.");
                    if (typeof (successCallback) === 'function')
                        successCallback(d && d.kwargs);
                }

                function error(e) {
                    $log.error((uri,parameters) + "Failed with error: '" + angular.toJson(e) + "'.");
                    if (e.error == 'wamp.quota_exhausted') {
                        apiQuotaExceeded = true;
                        return increaseRequestQuotaChallenge().then(function (captchaResponse) {
                            $wamp.call('/connection#increaseRequestQuota', { 'challengeResponse': captchaResponse }).then(function () {
                                apiQuotaExceeded = false;
                                $route.reload();
                            }, function (error) {
                                $log.error(error.kwargs.desc);
                            });
                        });
                    }
                    if (typeof (failureCallback) === 'function')
                        failureCallback(e.kwargs);
                }

                return originalFunc.call(callReturn, success, error);
            };

            return callReturn;
        };
        
        var challengeIsOpen = false;
        function renderChallenge() {
            var challengeDeferred = $q.defer();
            var challengePromise = challengeDeferred.promise;
            if (!challengeIsOpen) {
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
                    var bg = document.getElementById("bg");
                    bg.style.display = 'none';

                    onOpen();

                    challengeIsOpen = false;
                    challengeDeferred.resolve(renderedChallengeResponse);
                }, function (renderedChallengeError) {
                    vcRecaptchaService.reload();
                    $log.error(renderedChallengeError);
                    challengeIsOpen = false;
                    challengeDeferred.reject(renderedChallengeError);
                });
            }
            return challengePromise;
        };

        function increaseRequestQuotaChallenge() {
            return renderChallenge();
        };

        function logout() {
            _call("/user#logout");
        }

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
                return _call("/user/account#getCountries",
                {
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
                return _call("/user/email#sendVerificationEmailToNewMailbox",
                {
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

            logout: logout,

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
                return $wamp.call("/user/account#watchBalance").then(function(result) {
                    $wamp.subscribe("/account/balanceChanged", function (data) {
                        callback(data);
                    });
                });
            },
            unwatchBalance: function (account) {
                return $wamp.call("/user/account#unwatchBalance");
            },
            // #endregion

            // #region Init
            init: _init
            // #endregion
        };

        function onOpen(event, info) {
            $wamp.call('/connection#getClientIdentity').then(function (result) {
                localStorageService.set(constants.storageKeys.clientId, result.kwargs.cid);
            }, function (error) {
                $log.error(error.kwargs.desc);
            });
            $rootScope.$broadcast(constants.events.CONNECTION_INITIATED);
        }

        function _init() {
            var clientId = localStorageService.get(constants.storageKeys.clientId);
            $wamp.setClientId(clientId);

            $rootScope.$on("$wamp.open", onOpen);

            //$rootScope.$on("$wamp.close", function (event, info) {
            //    var c = $wamp.connection;
            //    var s = c.session;
            //});

            $rootScope.$on("$wamp.onchallenge", function (event, info) {
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

                    //var bg = document.getElementById("bg");
                    //bg.style.display = 'block';
                }

                var deferred = $q.defer();
                var promise = deferred.promise;
                if (info.method === 'GoogleRecaptchaV2')
                    return renderChallenge();
                else {
                    var err = 'Unknown challenge type: ' + info.method;
                    $log.error(err);
                    deferred.reject(err);
                }
                return promise;
            });

            $wamp.subscribe("/sessionStateChange", function (args, kwargs, details) {
                kwargs.initialized = $rootScope.initialized;
                //$rootScope.$broadcast(constants.events.CONNECTION_INITIATED);
                $rootScope.$broadcast(constants.events.SESSION_STATE_CHANGE, kwargs);
                $log.trace("SESSION_STATE_CHANGE => args: " + angular.toJson(args) + ", kwargs: " + angular.toJson(kwargs) + ", details: " + angular.toJson(details));
            });
            //.then(function (subscription) {
            //    var groupId = localStorageService.get(constants.storageKeys.clientId);
            //    _call("/user#setClientIdentity", { groupID: groupId || "" }).then(function (result) {
            //        $rootScope.$broadcast(constants.events.CONNECTION_INITIATED);
            //        localStorageService.set(constants.storageKeys.clientId, result.groupID);
            //    }, _logError);
            //    //var groupId = localStorageService.get(constants.storageKeys.clientId);
            //    //message.toastr('stored client id: ' + groupId);
            //    //if (groupId !== undefined) {
            //    //    _call("/user#setClientIdentity", { groupID: groupId }).then(function (result) {
            //    //        message.toastr('new client id: ' + result.groupID);
            //    //        localStorageService.set(constants.storageKeys.clientId, result.groupID);
            //    //    }, _logError);
            //    //}
            //}, _logError);

            // -------------------------------------------------------------------
            // This event is fired when the account balance is changed.
            // -------------------------------------------------------------------
            //
            // -------------------------------------------------------------------
            // Parameters
            // -------------------------------------------------------------------
            //  {
            //      "id" : 5510090,
            //      "vendor" : "CasinoWallet",
            //      "type" : "Deposit",
            //      "amount" : 50,
            //      "bonusAmount": 100
            //  }
            //
            // id [integer] 
            // the id of the balance-changed account
            //
            // vendor [string] 
            // the vendor of the account. please use this field to find specific account.
            //
            // type [string] 
            // a string represents the transaction type which triggers the balance change
            //      Deposit
            //      Withdraw
            //      Transfer
            //      User2User
            //      Vendor2User
            //      User2Vendor
            //      WalletCredit
            //      WalletDebit
            //      Refund
            //
            // amount [number]
            // the amount of the account.
            //
            // bonusAmount [number]
            // the bonus amount of the account
            //
            // -------------------------------------------------------------------
            // Remark
            // -------------------------------------------------------------------
            // This event is fired when the account balance is changed. 
            // Only the connection which called /user/account/watchBalance receives the notification on balance change.
            // This feature is only available after it is configured in GmCore, please ask your integration manager for details.
            // -------------------------------------------------------------------
            //$wamp.subscribe("/account/balanceChanged", function (args, kwargs, details) {
            //    $rootScope.$broadcast(constants.events.ACCOUNT_BALANCE_CHANGED, kwargs);

            //    console.log(
            //        "ACCOUNT_BALANCE_CHANGED => args: " + angular.toJson(args) +
            //        ", kwargs: " + angular.toJson(kwargs) +
            //        ", details: " + angular.toJson(details));
            //});

            //
            // This event is fired when the deposit transaction status is changed in 3rd-party site.
            //
            $wamp.subscribe("/deposit/statusChanged", function (args, kwargs, details) {
                $rootScope.$broadcast(constants.events.DEPOSIT_STATUS_CHANGED, args);

                $log.debug(
                    "DEPOSIT_STATUS_CHANGED => args: " + angular.toJson(args) +
                    ", kwargs: " + angular.toJson(kwargs) +
                    ", details: " + angular.toJson(details));
            });

            //
            // This event is fired when the withdraw transaction status is changed in 3rd-party site.
            //
            $wamp.subscribe("/withdraw/statusChanged", function (args, kwargs, details) {
                $rootScope.$broadcast(constants.events.WITHDRAW_STATUS_CHANGED, args);

                $log.debug(
                    "WITHDRAW_STATUS_CHANGED => args: " + angular.toJson(args) +
                    ", kwargs: " + angular.toJson(kwargs) +
                    ", details: " + angular.toJson(details));
            });

            $wamp.open();
        }

        return service;
    };

})();