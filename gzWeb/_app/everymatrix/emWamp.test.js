/// <reference path="emWamp.js" />

//https://gz.azureedge.net/scripts/autobahn.min.js

describe("emWamp service",
    function() {
        var emWamp;
        var originalTimeout;

        beforeEach(function() {
            module("GZ");

            var i = angular.injector(["ng"]),
                rs = i.get("$rootScope");

            flush = function() {
                rs.$apply();
            };

            module("GZ",
                function($provide) {
                    $provide.value("$rootScope", rs);
                });
        });

        beforeEach(function() {
            inject(function(_emWamp_) {
                emWamp = _emWamp_;
            });

            originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;
            jasmine.DEFAULT_TIMEOUT_INTERVAL = 30000;

            flush();
        });

        afterEach(function() {
            jasmine.DEFAULT_TIMEOUT_INTERVAL = originalTimeout;
        });

        it("should login valid user",
            function(done) {
                emWamp.login({ usernameOrEmail: 'gz2016', password: 'gz2016!@' })
                    .then(function(result) {
                            expect(result).toBeDefined();
                            expect(result.hasToAcceptTC).toBeDefined();
                            expect(result.minorChangeInTC).toBeDefined();
                            expect(result.majorChangeInTC).toBeDefined();
                            expect(result.isEmailVerified).toBeDefined();
                            expect(result.isProfileIncomplete).toBeDefined();
                            expect(result.roles).toBeDefined();
                            expect(result.hasToEnterCaptcha).toBeDefined();
                            expect(result.hasToChangePassword).toBeDefined();
                            expect(result.loginCount).toBeDefined();
                            expect(result.registrationTime).toBeDefined();
                            expect(result.lastLoginTime).toBeDefined();
                            done();
                        },
                        function() {
                            expect(false).toBe(true);
                            done();
                        });
                flush();
            });

        it("should not login invalid user",
            function(done) {
                emWamp.login({ usernameOrEmail: 'xxddxx', password: 'gz2016!@' })
                    .then(function(result) {
                            expect(false).toBe(true);
                            done();
                        },
                        function(error) {
                            expect(error).toBeDefined();
                            expect(error.desc).toBeDefined();
                            expect(error.desc).toBe('The login failed. Please check your username and password.');
                            done();
                        });
                flush();
            });

        it("should getSessionInfo",
            function(done) {
                emWamp.login({ usernameOrEmail: 'gz2016', password: 'gz2016!@' })
                    .then(function(_) {
                            emWamp.getSessionInfo()
                                .then(function(result) {
                                        expect(result).toBeDefined();
                                        expect(result.isAuthenticated).toBeDefined();
                                        expect(result.isAuthenticated).toBe(true);
                                        expect(result.firstname).toBeDefined();
                                        expect(result.surname).toBeDefined();
                                        expect(result.currency).toBeDefined();
                                        expect(result.userCountry).toBeDefined();
                                        expect(result.ipCountry).toBeDefined();
                                        expect(result.loginTime).toBeDefined();
                                        expect(result.isEmailVerified).toBeDefined();
                                        done();
                                    },
                                    function(error) {
                                        expect(false).toBe(true);
                                        done();
                                    });
                        },
                        function(error) {
                            expect(false).toBe(true);
                            done();
                        });
                flush();
            });

        it("should return a list of countries",
            function(done) {
                emWamp.getCountries()
                    .then(function(result) {
                            expect(result.currentIPCountry).toBeDefined();
                            expect(result.countries).toBeDefined();
                            done();
                        },
                        function(error) {
                            expect(false).toBe(true);
                            done();
                        });
                flush();
            });

        it("should return a list of currencies",
            function(done) {
                emWamp.getCurrencies()
                    .then(function(result) {
                            expect(result).toBeDefined();
                            expect(Array.isArray(result)).toBe(true);
                            done();
                        },
                        function(error) {
                            expect(false).toBe(true);
                            done();
                        });
                flush();
            });

    });