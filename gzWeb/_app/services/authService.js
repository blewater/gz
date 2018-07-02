﻿(function () {
    'use strict';

    APP.factory('auth', ['$rootScope', '$http', '$q', '$location', '$window', 'emWamp', 'emBanking', 'api', 'constants', 'localStorageService', 'helpers', 'vcRecaptchaService', 'iovation', '$log', '$filter', 'nav', '$route', 'message', '$timeout', authService]);

    function authService($rootScope, $http, $q, $location, $window, emWamp, emBanking, api, constants, localStorageService, helpers, vcRecaptchaService, iovation, $log, $filter, nav, $route, message, $timeout) {
        var factory = {};

        // #region AuthData
        var noAuthData = {
            firstname: "",
            lastname: "",
            currency: "",
            gamingAccounts: [],
            gamingAccount: undefined,
            username: "",
            token: "",
            roles: [constants.roles.guest],
            isGamer: false,
            isInvestor: false,
            isAdmin: false,
            isGuest: true,

            gdprConsents: undefined
        };
        
        factory.readAuthData = function () {
            factory.data = angular.extend(noAuthData, (localStorageService.get(constants.storageKeys.authData) || {}));
        }

        function storeAuthData() {
            localStorageService.set(constants.storageKeys.authData, factory.data);
        }

        function setGamingAuthData(sessionInfo) {
            var gamerIndex = factory.data.roles.indexOf(constants.roles.gamer);
            if (gamerIndex === -1)
                factory.data.roles.push(constants.roles.gamer);
            factory.data.isGamer = true;
            factory.data.firstname = sessionInfo.firstname;
            factory.data.lastname = sessionInfo.surname;
            factory.data.currency = sessionInfo.currency;
            storeAuthData();
        }

        function setInvestmentAuthData(data) {
            var investorIndex = factory.data.roles.indexOf(constants.roles.investor);
            if (investorIndex === -1)
                factory.data.roles.push(constants.roles.investor);
            factory.data.isInvestor = true;
            factory.data.username = data.userName;
            factory.data.token = data.access_token;

            factory.data.firstname = data.firstname;
            factory.data.lastname = data.lastname;
            factory.data.currency = data.currency;
            storeAuthData();
        }

        function getConsentValue(value) {
            var consentValue = value === undefined || value === null || value.length === 0 ? undefined : (value === true || value === "true");
            return consentValue;
        }
        function setGdprData(data) {
            factory.data.gdprConsents = {
                allowGzEmail: getConsentValue(data.allowGzEmail),
                allowGzSms: getConsentValue(data.allowGzSms),
                allow3rdPartySms: getConsentValue(data.allow3rdPartySms),
                acceptedGdprTc: getConsentValue(data.acceptedGdprTc),
                acceptedGdprPp: getConsentValue(data.acceptedGdprPp),
                acceptedGdpr3rdParties: getConsentValue(data.acceptedGdpr3rdParties)
            };
            storeAuthData();
        }

        function clearGamingData() {
            var gamerIndex = factory.data.roles.indexOf(constants.roles.gamer);
            if (gamerIndex > -1)
                factory.data.roles.splice(gamerIndex, 1);
            factory.data.isGamer = false;
            factory.data.gamingAccounts = [];
            factory.data.gamingAccount = undefined;
            factory.data.gamingBalance = undefined;
            if (!factory.data.isInvestor) {
                factory.data.firstname = "";
                factory.data.lastname = "";
                factory.data.currency = "";
            }
            localStorageService.remove(constants.storageKeys.clientId);
            storeAuthData();
        }

        function clearInvestmentData() {
            var investorIndex = factory.data.roles.indexOf(constants.roles.investor);
            if (investorIndex > -1)
                factory.data.roles.splice(investorIndex, 1);
            factory.data.isInvestor = false;
            factory.data.username = "";
            factory.data.token = "";
            if (!factory.data.isGamer) {
                factory.data.firstname = "";
                factory.data.lastname = "";
                factory.data.currency = "";
            }
            storeAuthData();
        }
        // #endregion

        // #region Session Management

        //function watchBalance() {
        //    if (factory.data.gamingAccount) {
        //        emWamp.watchBalance(factory.data.gamingAccount, function(data) {
        //            factory.data.gamingAccount.amount = data.amount;
        //            storeAuthData();
        //            $rootScope.$broadcast(constants.events.ACCOUNT_BALANCE_CHANGED);
        //        });
        //    } else
        //        console.log("watchBalance: No Gaming account to watch.");
        //}
        //function unwatchBalance() {
        //    if (factory.data.gamingAccount) {
        //        emWamp.unwatchBalance(factory.data.gamingAccount);
        //    } else
        //        console.log("unwatchBalance: No Gaming account to unwatch.");
        //}
        //function getGamingAccountAndWatchBalance() {
        //    if (!factory.data.gamingAccount) {
        //        emBanking.getGamingAccounts(true, false).then(function (result) {
        //            factory.data.gamingAccount = result.accounts[0];
        //            storeAuthData();
        //            $rootScope.$broadcast(constants.events.AUTH_CHANGED);
        //            watchBalance();
        //        }, function (error) {
        //            console.log(error.desc);
        //        });
        //    }
        //    else
        //        watchBalance();
        //}

        $rootScope.$on(constants.events.REQUEST_ACCOUNT_BALANCE, function (event, args) {
            var expectBalance = args && args.expectBalance ? args.expectBalance : true;
            var expectBonus = args && args.expectBonus ? args.expectBonus : true;
            var callback = args && args.callback ? args.callback : angular.noop;
            factory.getGamingAccounts(expectBalance, expectBonus, callback);
        });

        factory.getGamingAccounts = function (expectBalance, expectBonus, callback) {
            emBanking.getGamingAccounts(expectBalance, expectBonus).then(function (result) {
                factory.data.gamingAccounts = result.accounts;
                factory.data.gamingBalance = $filter('sum')($filter('map')(factory.data.gamingAccounts, function (acc) { return acc.amount; }));
                storeAuthData();
                $rootScope.$broadcast(constants.events.ACCOUNT_BALANCE_CHANGED);
                if (angular.isFunction(callback))
                    callback();

                //if (factory.data.gamingAccount.amount === currentBalance) {
                //    $timeout(function () {
                //        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                //    }, 1000);
                //}
                //else {
                //}
            }, function (error) {
                $log.error(error.desc);
            });
        };

        //function onSessionDisconnected() {
        //    clearGamingData();
        //    unwatchBalance();
        //    $rootScope.$broadcast(constants.events.SESSION_TERMINATED);
        //}

        factory.setGdpr = function (consents) {
            var q = $q.defer();
            //$timeout(function () {
            //    setGdprData(consents);
            //    q.resolve(true);
            //}, 3000);
            api.setGdpr(consents).then(function (result) {
                setGdprData(consents);
                q.resolve(true);
            }, function (error) {
                q.reject(error);
            });
            return q.promise;
        };
        function hasToSetConsents() {
            if (factory.data.gdprConsents === undefined)
                return true;
            else {
                for (var consent in factory.data.gdprConsents) {
                    var consentValue = factory.data.gdprConsents[consent];
                    if (consentValue === undefined || consentValue === null || consentValue.length === 0)
                        return true;
                }
                return false;
            }
        }
        $rootScope.$on(constants.events.SESSION_STATE_CHANGE, function (event, kwargs) {
            var args = kwargs;
            if (args.code === 0) {
                emWamp.getSessionInfo().then(function (sessionInfo) {
                    if (sessionInfo.isAuthenticated) {
                        setGamingAuthData(sessionInfo);
                        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                        //getGamingAccountAndWatchBalance();

                        if (args.initialized === true) { //&& $rootScope.routeData.category === constants.categories.wandering
                            var requestUrl = nav.getRequestUrl();
                            if (requestUrl) {
                                nav.clearRequestUrls();
                                $rootScope.$broadcast(constants.events.REDIRECTED);
                                $rootScope.redirected = false;
                                $location.path(requestUrl);
                            }
                            else if ($route.current.$$route.originalPath === constants.routes.home.path && factory.data.isGamer)
                                $location.path(constants.routes.games.path).search({});
                            else if ($route.current.$$route.originalPath === constants.routes.home.path && factory.data.isInvestor)
                                $location.path(constants.routes.summary.path);
                        }
                        $rootScope.$broadcast(constants.events.AUTH_CHANGED);
                    }
                    else {
                        factory.logout();
                        //emLogout();
                    }
                });
            }
            else if (args.code > 0) {
                factory.onLogout(args.desc);
            }
        });

        //var requestPath = undefined;
        //factory.nonAuthRedirect = function () {
        //    var path = $location.url();
        //    if (path && path !== constants.routes.home.path)
        //        requestPath = path;
        //    $location.path(constants.routes.home.path);
        //};
        //factory.loginRedirect = function () {
        //    if (angular.isDefined(requestPath))
        //        $location.path(requestPath);
        //    else if (factory.data.isGamer)
        //        $location.path(constants.routes.games.path).search({});
        //    else if (factory.data.isInvestor)
        //        $location.path(constants.routes.summary.path);
        //};
        //var currentRoute = $route.current.$$route;
        //if (!auth.authorize(currentRoute.roles))
        //    $location.path(constants.routes.home.path);
        //else
        //    setRouteData(currentRoute);

        // #endregion

        // #region Authorize
        factory.authorize = function (roles, mode) {
            if (roles && factory.data) {
                var rolesArray = angular.isArray(roles) ? roles : roles.split(',');
                var contains = function (r) {
                    return factory.data.roles.indexOf(r) !== -1;
                }
                if (mode === 'all')
                    return helpers.array.all(rolesArray, contains);
                else if (mode === 'exact')
                    return factory.data.roles.length === rolesArray.length && helpers.array.all(rolesArray, contains);
                else
                    return helpers.array.any(rolesArray, contains);
            }
            else
                return false;
        };
        // #endregion

        // #region Logout
        factory.logout = function () {
            if (factory.data.isGamer)
                emWamp.logout();
            else
                factory.onLogout();
        };

        function setLogout() {
            clearInvestmentData();
            nav.clearRequestUrls();
            message.clear();

            clearGamingData();
            $rootScope.$broadcast(constants.events.AUTH_CHANGED);
        };
        factory.onLogout = function (reason) {
            setLogout();

            //$location.path(constants.routes.home.path).search({ logoutReason: reason });
            window.appInsights.trackEvent("LOGOUT");
            $window.location.href = constants.routes.home.path + (reason ? ("?logoutReason=" + reason) : "");
        }
        // #endregion

        // #region Login
        factory.gzLogin = function(usernameOrEmail, password) {
            var q = $q.defer();

            api.login(usernameOrEmail, password).then(function (gzLoginResult) {
                setInvestmentAuthData(gzLoginResult.data);
                setGdprData(gzLoginResult.data);
                q.resolve(gzLoginResult);
            }, function (error) {
                q.reject(error.data.error_description);
            });
            return q.promise;
        }

        // Called after a login response prop hasToAcceptTC is set = true
        function emAcceptTcPromise() {
            var q = $q.defer();

            emWamp.acceptTC().then(function () {
                q.resolve();
            }, function (error) {
                q.reject(error ? error.desc : false);
            });
            return q.promise;
        }
        // Called after a login response prop hasToSetUserConsent is set = true
        function emGetUserConsent(hasToSetUserConsent, action) {
            var q = $q.defer();

            if (hasToSetUserConsent) {
                var params = angular.extend({}, { "action": action });

                emWamp.getConsentRequirements(params).then(function(consentList) {
                        q.resolve(consentList);
                    },
                    function(error) {
                        q.reject(error ? error.desc : false);
                    });
            } else {
                q.resolve();
            }
            return q.promise;
        }
        // action: 1 : registration, 2: login, 3: profile
        function emGetActionableUserConsent(action) {
            var q = $q.defer();
            var params = angular.extend({}, { "action": action });

            emWamp.getConsentRequirements(params).then(function(consentList) {
                q.resolve(consentList);
            },
            function(error) {
                q.reject(error ? error.desc : false);
            });

            return q.promise;
        }
        factory.setUserConsentQuestions = function(controller, action) {
            var q = $q.defer();

            emGetActionableUserConsent(action).then(function(userConsentQuestions) {
                if (userConsentQuestions) {
                    angular.forEach(userConsentQuestions,
                        function(value) {
                            if (value.code.indexOf(constants.emUserConsentKeys.tcApiCode) !== -1) {
                                // may be set by getConsentRequirements or hasToAcceptTC
                                controller.showTcbyUserConsentApi = true;
                            }

                            if (value.code.indexOf(constants.emUserConsentKeys.emailApiCode) !== -1) {
                                controller.showEmail = true;
                            }

                            if (value.code.indexOf(constants.emUserConsentKeys.smsApiCode) !== -1) {
                                controller.showSms = true;
                            }

                            if (value.code.indexOf(constants.emUserConsentKeys.thirdpartyApiCode) !== -1) {
                                controller.show3rdParty = true;
                            }
                        });
                    // if null
                } else {
                    controller.showTcbyUserConsentApi = true;
                }
                q.resolve(userConsentQuestions);
            },
            function(errorGetUserConsent) {
                q.reject(errorGetUserConsent ? errorGetUserConsent.desc : false);
            });
            return q.promise;
        }

        factory.isGdprFormSectionValid = function (controller) {
            var consObj = controller.consents;
            var acceptedGdprTc = consObj.acceptedGdprTc;

            // TC by either accept or userconsent Api
            if (controller.showTcbyUserConsentApi && (acceptedGdprTc === undefined || acceptedGdprTc === "false"))
                return false;

            if (controller.showEmail && consObj.allowGzEmail === undefined)
                return false;

            if (controller.showSms && consObj.allowGzSms === undefined)
                return false;

            if (controller.show3rdParty && consObj.allow3rdPartySms === undefined)
                return false;

            return true;
        };

        // create userConsents for inclusion in Api params call (reg., login, profile)
        factory.createUserConsents = function(controller) {

            var userConsents = undefined;
            if (controller.consents.isUc) {
                userConsents = {};
                var consents = controller.consents;
                if (controller.showTcbyUserConsentApi) {
                    userConsents[constants.emUserConsentKeys.tcApiCode] = consents.acceptedGdprTc;
                }
                if (controller.showEmail) {
                    userConsents[constants.emUserConsentKeys.emailApiCode] = consents.allowGzEmail;
                }
                if (controller.showSms) {
                    userConsents[constants.emUserConsentKeys.smsApiCode] = consents.allowGzSms;
                }
                if (controller.show3rdParty) {
                    userConsents[constants.emUserConsentKeys.thirdpartyApiCode] = consents.allow3rdPartySms;
                }
            }
            return userConsents;
        }

        // Called after getUserConsents
        function emSetUserConsent(userConsents) {
            var q = $q.defer();

            emWamp.setConsentRequirements({ userConsents: userConsents }).then(function() {
                    q.resolve();
                },
                function(setUserConsentError) {
                    q.reject(setUserConsentError ? setUserConsentError.desc : false);
                });

            return q.promise;
        }
        function condSetUserConsents(popupConsents) {
            var q = $q.defer();
            if (popupConsents.setUserConsent) {
                emSetUserConsent(popupConsents.userConsents).then(function() {
                        q.resolve();
                    },
                    function(acceptTcError) {
                        q.reject(acceptTcError ? acceptTcError.desc : false);
                    });
            } else {
                q.resolve();
            }
            return q.promise;
        }
        // handle any flag set 2 true for hasToSetUserConsent, hasToAcceptTC and minorChangeInTC
        function conditionalConsentPopup(emLoginResult) {
            var q = $q.defer();

            // Prod may not be synced with the login api update
            if (emLoginResult.hasToSetUserConsent === undefined) 
                emLoginResult.hasToSetUserConsent = false;
            emGetUserConsent(emLoginResult.hasToSetUserConsent, 2).then(function(userConsentList) {

                if (emLoginResult.hasToAcceptTC || emLoginResult.hasToSetUserConsent) {
                    message.open({
                        nsType: 'modal',
                        nsSize: 'md',
                        nsTemplate: '_app/account/gdpr.html',
                        nsCtrl: 'gdprCtrl',
                        nsStatic: true,
                        nsShowClose: false,
                        nsParams: {
                            isTc: emLoginResult.hasToAcceptTC,
                            isUc: emLoginResult.hasToSetUserConsent,
                            userConsentList: userConsentList
                        }
                    }).then(function(popupConsents) {
                            // Have to call acceptTC?
                            if (popupConsents.setAcceptTC) {
                                emAcceptTcPromise().then(function() {
                                    // Then call setUserConsents?
                                    condSetUserConsents(popupConsents).then(function() {
                                        q.resolve();
                                    }, function(setUserConsents) {
                                        q.reject(setUserConsents ? setUserConsents.desc : false);
                                    });
                                }, function(acceptTcError) {
                                    q.reject(acceptTcError ? acceptTcError.desc : false);
                                });
                            } else {
                                // --Or just call setUserConsents?
                                condSetUserConsents(popupConsents).then(function() {
                                    q.resolve();
                                }, function(setUserConsents) {
                                    q.reject(setUserConsents ? setUserConsents.desc : false);
                                });
                            }
                        },
                        function(errorPopup) {
                            q.reject(errorPopup ? errorPopup.desc : false);
                        });
                } else if (emLoginResult.minorChangeInTC) {
                    message.notify("The Terms and Conditions have minor changes. Click here to check the details.",
                        {
                            nsCallback: function() {
                                $location.path(constants.routes.termsGames.path);
                                q.resolve();
                            }
                        });
                } else {
                    q.resolve();
                }
            }, function(errorGetConsent) {
                q.reject(errorGetConsent ? errorGetConsent.desc : false);
            });
            return q.promise;
        }

        function emLogin(usernameOrEmail, password, captcha, widgetId) {
            var q = $q.defer();
            var params = {
                usernameOrEmail: usernameOrEmail,
                password: password,
                iovationBlackbox: iovation.getBlackbox()
            };
            if (captcha) {
                params.captchaPublicKey = localStorageService.get(constants.storageKeys.reCaptchaPublicKey);
                params.captchaChallenge = "";
                params.captchaResponse = vcRecaptchaService.getResponse(widgetId);
            }

            emWamp.login(params).then(function (emLoginResult) {
                factory.data.username = usernameOrEmail;
                q.resolve(emLoginResult);
            }, function (error) {
                factory.data.username = '';
                q.reject(error ? error.desc : false);
            });
            return q.promise;
        }

        factory.login = function (usernameOrEmail, password, captcha, widgetId) {
            var q = $q.defer();
            emLogin(usernameOrEmail, password, captcha, widgetId).then(function (emLoginResult) {
                window.appInsights.setAuthenticatedUserContext(usernameOrEmail);
                window.appInsights.trackEvent("LOGIN");
                if (emLoginResult.hasToEnterCaptcha)
                    q.resolve({ enterCaptcha: true });
                else {
                    message.clear();

                    factory.gzLogin(usernameOrEmail, password).then(function () {
                        conditionalConsentPopup(emLoginResult).then(function() {
                            $rootScope.$broadcast(constants.events.AUTH_CHANGED);
                            api.cacheUserData();
                            q.resolve({ emLogin: true, gzLogin: true });
                        }, function(popupError) {
                            $log.error("Greenzorro login popup failed for user " + usernameOrEmail + ": " + popupError);
                            q.resolve({ emLogin: false, gzLogin: true });
                        });
                    }, function (gzLoginError) {
                        $log.error("Greenzorro login failed for user " + usernameOrEmail + ": " + gzLoginError);
                        q.resolve({ emLogin: true, gzLogin: false });
                    });
                }
            }, function (emLoginError) {
                //gzLogin(usernameOrEmail, password).then(function () {
                //    api.cacheUserData();
                //    q.resolve({ emLogin: false, emError: emLoginError, gzLogin: true });
                //}, function (gzLoginError) {
                //    q.resolve({ emLogin: false, emError: emLoginError, gzLogin: false, gzError: gzLoginError });
                //});
                window.appInsights.setAuthenticatedUserContext('');
                q.resolve({ emLogin: false, emError: emLoginError });
            });
            return q.promise;
        }
        // #endregion

        // #region Register
        factory.readBtag = function () {
            var btag = $location.search().btag;
            if (btag) {
                var now = new Date();
                localStorageService.set(constants.storageKeys.btagMarker, btag);
                localStorageService.set(constants.storageKeys.btagTime, now.getTime());
            }
        };

        function clearBtags() {
            localStorageService.remove(constants.storageKeys.btagMarker);
            localStorageService.remove(constants.storageKeys.btagTime);
        };
        function getBtag() {
            var btag = localStorageService.get(constants.storageKeys.btagMarker);
            var btagTime = localStorageService.get(constants.storageKeys.btagTime);
            var now = new Date();
            if (btag && btagTime && btagTime <= now.getTime() + constants.keepBtagAliveTime)
                return btag;

            clearBtags();
            return "";
        };

        function emRegister(parameters) {
            return emWamp.register({
                username: parameters.username,
                email: parameters.email,
                alias: parameters.username,
                password: parameters.password,
                firstname: parameters.firstname,
                surname: parameters.lastname,
                birthDate: parameters.dateOfBirthFormatted,
                country: parameters.country,
                // TODO: region 
                // TODO: personalID 
                mobilePrefix: parameters.mobilePrefix,
                mobile: parameters.mobile,
                currency: parameters.currency,
                title: parameters.title,
                gender: parameters.gender,
                city: parameters.city,
                address1: parameters.address1,
                address2: parameters.address2,
                postalCode: parameters.postalCode,
                language: parameters.language,
                emailVerificationURL: parameters.emailVerificationURL,
                securityQuestion: parameters.securityQuestion,
                securityAnswer: parameters.securityAnswer,
                iovationBlackbox: iovation.getBlackbox(),
                affiliateMarker: getBtag(),
                userConsents: parameters.userConsents
            });
        }

        function gzRegister(parameters) {
            return api.register({
                Username: parameters.username,
                Email: parameters.email,
                Password: parameters.password,
                FirstName: parameters.firstname,
                LastName: parameters.lastname,
                Birthday: parameters.dateOfBirth,
                Currency: parameters.currency,
                Title: parameters.title,
                Country: parameters.country,
                MobilePrefix: parameters.mobilePrefix,
                Mobile: parameters.mobile,
                City: parameters.city,
                Address: parameters.address1,
                PostalCode: parameters.postalCode,
                // TODO: Region ???
                AcceptedGdprTc: parameters.userConsents[constants.emUserConsentKeys.tcApiCode],
                AllowGzEmail: parameters.userConsents[constants.emUserConsentKeys.emailApiCode],
                AllowGzSms: parameters.userConsents[constants.emUserConsentKeys.smsApiCode],
                Allow3rdPartySms: parameters.userConsents[constants.emUserConsentKeys.thirdpartyApiCode]
            });
        }

        factory.register = function (parameters) {
            var q = $q.defer();

            var revoke = function(error) {
                api.revokeRegistration(error).then(function () {
                    q.reject(error);
                }, function () {
                    q.reject(error);
                });
            }

            var registrationSteps = ['gzRegister', 'gzLogin', 'emRegister', 'emLogin', 'getSessionInfo', 'finalizeRegistration'];
            function logStepMsg(name) {
                var step = registrationSteps.indexOf(name) + 1;
                return "Registration step " + step + " (" + name + ")";
            }

            function logStepSucceeded(name, user) {
                $log.info(logStepMsg(name) + " for user " + user + " succeeded!");
            }

            function logStepFailed(name, msg) {
                $log.error(logStepMsg(name) + ": " + msg);
            }

            gzRegister(parameters).then(function (gzRegisterResult) {
                var user = parameters.email || parameters.username;
                logStepSucceeded("gzRegister", user);
                factory.gzLogin(parameters.username, parameters.password).then(function (gzLoginResult) {
                    logStepSucceeded("gzLogin", user);
                    emRegister(parameters).then(function (emRegisterResult) {
                        logStepSucceeded("emRegister", user);
                        clearBtags();
                        emLogin(parameters.username, parameters.password).then(function (emLoginResult) {
                            logStepSucceeded("emLogin", user);
                            emWamp.getSessionInfo().then(function (sessionInfo) {
                                logStepSucceeded("getSessionInfo", user);
                                api.finalizeRegistration(sessionInfo.userID).then(function () {
                                    logStepSucceeded("finalizeRegistration", user);
                                    q.resolve(true);
                                }, function (finalizeError) {
                                    logStepFailed("finalizeRegistration", finalizeError);
                                    q.reject(finalizeError);
                                });
                            }, function (getSessionInfoError) {
                                logStepFailed("getSessionInfo", getSessionInfoError);
                                q.reject(getSessionInfoError);
                            });
                        }, function (emLoginError) {
                            var emLoginErr = emLoginError || "Undefined error. Please try logging in again.";
                            logStepFailed("emLogin", emLoginErr);
                            q.reject(emLoginErr);
                        });
                    }, function (emRegisterError) {
                        var emRegisterErr = emRegisterError ? emRegisterError.desc : "Undefined error. Please try again later.";
                        logStepFailed("emRegister", emRegisterErr);
                        revoke(emRegisterErr);

                        //var errorDesc = emRegisterError.desc;
                        //logStepFailed("emRegister", errorDesc);
                        //// when system is busy, the Em registration likely succeeded; don't revoke
                        //if (errorDesc.indexOf('system is busy now') === -1 ) {
                        //    revoke(emRegisterError.desc);
                        //}
                    });
                }, function (gzLoginError) {
                    var gzLoginErr = gzLoginError || "Login error.";
                    logStepFailed("gzLogin", gzLoginErr);
                    q.reject(gzLoginErr);
                    //revoke(gzLoginError.desc);
                });
            }, function(gzRegisterError) {
                logStepFailed("gzRegister", gzRegisterError.data.Message);
                q.reject(gzRegisterError.data.Message);
            });

            return q.promise;
        }
        // #endregion

        // #region ForgotPassword
        factory.forgotPassword = function (email, widgetId) {
            var q = $q.defer();
            api.forgotPassword(email).then(function (gzResetKey) {
                var changePwdUrl = $location.protocol() + "://" + $location.host();
                if ($location.port() > 0)
                    changePwdUrl += ":" + $location.port();

                changePwdUrl += "?";
                changePwdUrl += "email=";
                changePwdUrl += email;
                changePwdUrl += "&";
                changePwdUrl += "gzKey=";
                changePwdUrl += encodeURIComponent(gzResetKey.data);
                changePwdUrl += "&";
                changePwdUrl += "emKey=";

                emWamp.sendResetPwdEmail({
                    email: email,
                    changePwdURL: changePwdUrl,
                    captchaPublicKey: localStorageService.get(constants.storageKeys.reCaptchaPublicKey),
                    captchaChallenge: "",
                    captchaResponse: vcRecaptchaService.getResponse(widgetId)
                }).then(function (result) {
                    q.resolve(true);
                }, function (error) {
                    vcRecaptchaService.reload();
                    q.reject(error.desc);
                });
            }, function (error) {
                vcRecaptchaService.reload();
                q.reject(error);
            });
            return q.promise;
        }
        // #endregion

        // #region ResetPassword
        factory.resetPassword = function (parameters) {
            var q = $q.defer();
            emWamp.resetPassword(parameters.emKey, parameters.password).then(function (emResetResult) {
                api.resetPassword({
                    Email: parameters.email,
                    Password: parameters.password,
                    ConfirmPassword: parameters.confirmPassword,
                    Code: parameters.gzKey
                }).then(function(gzResetResult) {
                    q.resolve(true);
                }, function(gzResetError) {
                    q.reject(gzResetError);
                });
            }, function (emResetError) {
                q.reject(emResetError.desc);
            });
            return q.promise;
        }
        // #endregion

        // #region ChangePassword
        factory.changePassword = function (oldPassword, newPassword, confirmPassword, widgetId) {
            var q = $q.defer();
            emWamp.changePassword({
                oldPassword: oldPassword,
                newPassword: newPassword,
                captchaPublicKey: localStorageService.get(constants.storageKeys.reCaptchaPublicKey),
                captchaChallenge: "",
                captchaResponse: vcRecaptchaService.getResponse(widgetId)
            }).then(function (emChangeResult) {
                api.changePassword({
                    OldPassword: oldPassword,
                    NewPassword: newPassword,
                    ConfirmPassword: confirmPassword
                }).then(function (gzChangeResult) {
                    q.resolve(true);
                }, function (gzChangeError) {
                    vcRecaptchaService.reload();
                    q.reject(gzChangeError);
                });
            }, function (emChangeError) {
                vcRecaptchaService.reload();
                q.reject(emChangeError.desc);
            });
            return q.promise;
        }
        // #endregion

        // #region Bonus
        factory.applyBonus = function (bonusCode) {
            return emWamp.applyBonus({
                bonusCode: bonusCode,
                iovationBlackbox: iovation.getBlackbox()
            });
        }
        factory.getGrantedBonuses = function () {
            return emWamp.getGrantedBonuses();
        }
        factory.getApplicableBonuses = function (parameters) {
            return emWamp.getApplicableBonuses(parameters);
        }
        factory.forfeit = function (bonusID) {
            return emWamp.forfeit(bonusID);
        }
        factory.moveToTop = function (bonusID) {
            return emWamp.moveToTop(bonusID);
        }
        // #endregion

        // #region Init
        factory.init = function () {
            factory.readAuthData();
            factory.readBtag();

            api.call(function () {
                return api.getDeploymentInfo();
            }, function (response) {
                //var lastVersion = localStorageService.get(constants.storageKeys.version);
                //if (lastVersion !== response.Result.Version)
                //    setLogout();

                localStorageService.set(constants.storageKeys.version, response.Result.Version);
                localStorageService.set(constants.storageKeys.debug, response.Result.Debug);
                localStorageService.set(constants.storageKeys.reCaptchaPublicKey, response.Result.ReCaptchaSiteKey);
                //localStorageService.set(constants.storageKeys.live, response.Result.Live);
            });

            var unregisterConnectionInitiated = $rootScope.$on(constants.events.CONNECTION_INITIATED, function () {
                if (factory.data.username.length > 0) {
                    emWamp.getSessionInfo().then(function (sessionInfo) {
                        if (!sessionInfo.isAuthenticated)
                            setLogout();
                        $rootScope.$broadcast(constants.events.ON_INIT);
                    });
                }
                else
                    $rootScope.$broadcast(constants.events.ON_INIT);
                unregisterConnectionInitiated();
            });

            emWamp.init();
        };
        // #endregion

        return factory;
    };
})();