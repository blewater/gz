(function () {
    'use strict';
    var ctrlId = 'registerCtrl';
    APP.controller(ctrlId, ['$scope', 'emService', ctrlFactory]);
    function ctrlFactory($scope, emService) {
        $scope.model = {
            email: null,
            password: null,
            dateOfBirth: null
        };

        //$scope.$on('$wamp.open', function(event, session) {
        //    console.log('We are connected to the WAMP Router!');
        //});

        //$scope.$on("$wamp.close", function (event, data) {
        //    $scope.reason = data.reason;
        //    $scope.details = data.details;
        //    console.log('We are connected to the WAMP Router!');
        //});

        $scope.validateEmail = function() {
            emService.validateEmail($scope.model.email).then(function(result) {
                if (result.isAvailable)
                    console.log("email is available");
                else
                    console.log("email not available: " + result.error);
            });
        };
        
        emService.getCountries().then(function (result) {
            console.log("getCountries => ");
            console.log(result);
        });

        emService.getCurrencies().then(function (result) {
            console.log("getCurrencies => ");
            console.log(result);
        });

        emService.ensureRegistrationIsAllowed().then(function (result) {
            console.log("ensureRegistrationIsAllowed => ");
            console.log(result);
        });

        emService.login('ppet', '1q2w3e4r!@#$').then(
            function(result) {
                console.log("login => ");
                console.log(result);

                emService.getUserBasicConfig().then(
                    function(result1) {
                        console.log("getUserBasicConfig => ");
                        console.log(result1);

                    },
                    function(error) {
                        console.log(error);
                    });
            },
            function(error) {
                console.log(error);
            });
    }

    APP.factory("emService", ['$wamp', 'constants', function ($wamp, constants) {

        var wrapWampCall = function(uri, parameters) {

            var callReturn = $wamp.call(uri, [], parameters);

            var originalFunc = callReturn.then;
            callReturn.then = function (successCallback, failureCallback) {
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
            
            ensureRegistrationIsAllowed: function() {
                return wrapWampCall('/user/account#ensureRegistrationIsAllowed');
            },

            validateUsername : function(username) {
                return wrapWampCall('/user/account#validateUsername', {
                    username: username
                });
            },

            validateEmail: function (email) {
                return wrapWampCall('/user/account#validateEmail', {
                    email: email
                });
            },

            register: function (email, password, birthDate, currency) {
                return wrapWampCall('/user/account#register', {
                    username: email,
                    email: email,
                    alias: email,
                    password: password,
                    birthDate: birthDate,
                    currency: currency,
                    emailVerificationURL: constants.emailVerificationUrl
                });
            },

            getUserBasicConfig: function() {
                return wrapWampCall('/user/basicConfig#get');
            },

            login: function(username, password) {
                return wrapWampCall('/user#login', {
                    usernameOrEmail: username,
                    password: password,
                });
            },

            logout: function() {
                return wrapWampCall('/user#logout');
            },

            getCountries: function() {
                return wrapWampCall('/user/account#getCountries', {
                    expectRegions: false, // true
                    filterByCountry: '',
                    excludeDenyRegistrationCountry: false //true
                });
            },

            getCurrencies: function () {
                return wrapWampCall('/user/account#getCurrencies');
            }
        };

        $wamp.open();

        return service;
    }]);

})();