﻿(function () {
    'use strict';

    APP.factory('auth', ['$rootScope', '$http', '$q', '$location', '$window', 'emWamp', 'emBanking', 'api', 'constants', 'localStorageService', 'helpers', 'vcRecaptchaService', authService]);

    function authService($rootScope, $http, $q, $location, $window, emWamp, emBanking, api, constants, localStorageService, helpers, vcRecaptchaService) {
        var factory = {};

        // #region AuthData
        var noAuthData = {
            firstname: "",
            lastname: "",
            currency: "",
            gamingAccount: undefined,
            username: "",
            token: "",
            roles: [constants.roles.guest],
            isGamer: false,
            isInvestor: false,
            isAdmin: false,
            isGuest: true
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

        function clearGamingData() {
            var gamerIndex = factory.data.roles.indexOf(constants.roles.gamer);
            if (gamerIndex > -1)
                factory.data.roles.splice(gamerIndex, 1);
            factory.data.isGamer = false;
            factory.data.gamingAccount = undefined;
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
        function watchBalance() {
            if (factory.data.gamingAccount) {
                emWamp.watchBalance(factory.data.gamingAccount, function(data) {
                    factory.data.gamingAccount.amount = data.amount;
                    storeAuthData();
                    $rootScope.$broadcast(constants.events.ACCOUNT_BALANCE_CHANGED);
                });
            } else
                console.log("watchBalance: No Gaming account to watch.");
        }
        function unwatchBalance() {
            if (factory.data.gamingAccount) {
                emWamp.unwatchBalance(factory.data.gamingAccount);
            } else
                console.log("unwatchBalance: No Gaming account to unwatch.");
        }
        function getGamingAccountAndWatchBalance() {
            if (!factory.data.gamingAccount) {
                emBanking.getGamingAccounts(true, false).then(function (result) {
                    factory.data.gamingAccount = result.accounts[0];
                    storeAuthData();
                    $rootScope.$broadcast(constants.events.AUTH_CHANGED);
                    watchBalance();
                }, function (error) {
                    console.log(error.desc);
                });
            }
            else
                watchBalance();
        }

        //function onSessionDisconnected() {
        //    clearGamingData();
        //    unwatchBalance();
        //    $rootScope.$broadcast(constants.events.SESSION_TERMINATED);
        //}

        $rootScope.$on(constants.events.SESSION_STATE_CHANGE, function (event, kwargs) {
            var args = kwargs;
            if (args.code === 0) {
                emWamp.getSessionInfo().then(function (sessionInfo) {
                    if (sessionInfo.isAuthenticated) {
                        setGamingAuthData(sessionInfo);
                        getGamingAccountAndWatchBalance();

                        if (args.initialized === true && $rootScope.routeData.category === constants.categories.wandering) {
                            if (factory.data.isGamer)
                                $location.path(constants.routes.games.path);
                            else if (factory.data.isInvestor)
                                $location.path(constants.routes.summary.path);
                        }
                        $rootScope.$broadcast(constants.events.AUTH_CHANGED);
                    }
                    else {
                        emLogout();
                    }
                });
            }
            //else if (args.code === 2) {
            //}
            else {
                // TODO Check other codes
                emLogout();
                //$location.path(constants.routes.home.path);
                //$rootScope.$broadcast(constants.events.AUTH_CHANGED);
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
        //        $location.path(constants.routes.games.path);
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
            if (roles) {
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
        function gzLogout() {
            clearInvestmentData();
        }
        function emLogout() {
            clearGamingData();
            unwatchBalance();
            emWamp.logout();
            //$location.path(constants.routes.home.path);
            //$window.location.reload();
            $window.location.href = constants.routes.home.path;
        }

        factory.logout = function () {
            gzLogout();
            emLogout();
            //$rootScope.$broadcast(constants.events.AUTH_CHANGED);

            // TODO
            //var templates = $filter('toArray')(constants.templates);
            //for (var i = 0; i < templates.length; i++)
            //    $templateCache.remove(templates[i]);
            //message.clear();
        }
        // #endregion

        // #region Login
        function gzLogin(usernameOrEmail, password) {
            var q = $q.defer();

            api.login(usernameOrEmail, password).then(function (gzLoginResult) {
                setInvestmentAuthData(gzLoginResult.data);
                $rootScope.$broadcast(constants.events.AUTH_CHANGED);
                q.resolve(gzLoginResult);
            }, function (error) {
                q.reject(error.data.error_description);
            });
            return q.promise;
        }

        function emLogin(usernameOrEmail, password, captcha) {
            var q = $q.defer();
            var params = {
                usernameOrEmail: usernameOrEmail,
                password: password
            };
            if (captcha) {
                params.captchaPublicKey = constants.reCaptchaPublicKey;
                params.captchaChallenge = "";
                params.captchaResponse = vcRecaptchaService.getResponse();
            }

            emWamp.login(params).then(function (emLoginResult) {
                q.resolve(emLoginResult);
            }, function (error) {
                q.reject(error ? error.desc : false);
            });
            return q.promise;
        }

        factory.login = function (usernameOrEmail, password, captcha) {
            var q = $q.defer();
            emLogin(usernameOrEmail, password, captcha).then(function(emLoginResult) {
                //for (var i = 0; i < emLoginResult.roles.length; i++)
                //    console.log("==========> EveryMatrix Role " + i + ": " + emLoginResult.roles[i]);
                //factory.data.push(emLoginResult.roles[i]);
                //storeAuthData();
                if (emLoginResult.hasToEnterCaptcha)
                    q.resolve({ enterCaptcha: true });
                else {
                    gzLogin(usernameOrEmail, password).then(function () {
                        //$location.path(constants.routes.games.path);
                        q.resolve({ emLogin: true, gzLogin: true });
                    }, function (gzLoginError) {
                        //$location.path(constants.routes.games.path);
                        q.resolve({ emLogin: true, gzLogin: false, gzError: gzLoginError });
                    });
                }
            }, function (emLoginError) {
                gzLogin(usernameOrEmail, password).then(function () {
                    //$location.path(constants.routes.summary.path);
                    q.resolve({ emLogin: false, emError: emLoginError, gzLogin: true });
                }, function (gzLoginError) {
                    q.resolve({ emLogin: false, emError: emLoginError, gzLogin: false, gzError: gzLoginError });
                });
            });
            return q.promise;
        }
        // #endregion

        // #region Register
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
                securityAnswer: parameters.securityAnswer
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
            });
        }

        factory.register = function (parameters) {
            var q = $q.defer();

            var revoke = function(error) {
                api.revokeRegistration().then(function () {
                    q.reject(error);
                }, function () {
                    q.reject(error);
                });
            }

            gzRegister(parameters).then(function (gzRegisterResult) {
                gzLogin(parameters.username, parameters.password).then(function (gzLoginResult) {
                    emRegister(parameters).then(function (emRegisterResult) {
                        emLogin(parameters.username, parameters.password).then(function (emLoginResult) {
                            emWamp.getSessionInfo().then(function (sessionInfo) {
                                api.finalizeRegistration(sessionInfo.userID).then(function () {
                                    q.resolve(true);
                                }, function (finalizeError) {
                                    q.reject(finalizeError);
                                });
                            }, function (getSessionInfoError) {
                                q.reject(getSessionInfoError);
                            });
                        }, function (emLoginError) {
                            q.reject(emLoginError);
                        });
                    }, function (emRegisterError) {
                        revoke(emRegisterError.desc);
                    });
                }, function (gzLoginError) {
                    revoke(gzLoginError.desc);
                });
            }, function(gzRegisterError) {
                q.reject(gzRegisterError.data.Message);
            });

            return q.promise;
        }
        // #endregion

        // #region ForgotPassword
        factory.forgotPassword = function (email) {
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
                    captchaPublicKey: constants.reCaptchaPublicKey,
                    captchaChallenge: "",
                    captchaResponse: vcRecaptchaService.getResponse()
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
        factory.changePassword = function (oldPassword, newPassword, confirmPassword) {
            var q = $q.defer();
            emWamp.changePassword({
                oldPassword: oldPassword,
                newPassword: newPassword,
                captchaPublicKey: constants.reCaptchaPublicKey,
                captchaChallenge: "",
                captchaResponse: vcRecaptchaService.getResponse()
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

        // #region Init
        factory.init = function () {
            factory.readAuthData();

            api.call(function () {
                return api.getDeploymentInfo();
            }, function (response) {
                localStorageService.set(constants.storageKeys.version, response.Result.Version);
                localStorageService.set(constants.storageKeys.debug, response.Result.Debug);
            });

            emWamp.init();
            var unregisterConnectionInitiated = $rootScope.$on(constants.events.CONNECTION_INITIATED, function () {
                if (factory.data.username.length > 0) {
                    emWamp.getSessionInfo().then(function (sessionInfo) {
                        if (!sessionInfo.isAuthenticated)
                            factory.logout();
                    });
                }

                $rootScope.$broadcast(constants.events.ON_INIT);
                unregisterConnectionInitiated();
            });
        };
        // #endregion

        return factory;
    };
})();