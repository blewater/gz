(function () {
    'use strict';
    var ctrlId = 'registerCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', ctrlFactory]);
    function ctrlFactory($scope, emWamp) {
        $scope.model = {
            email: null,
            password: null,
            dateOfBirth: null
        };

        $scope.usernameValid = {};
        $scope.emailValid = {};
        $scope.currentIpCountry = '';
        $scope.countries = [];
        $scope.currencies = [];

        $scope.validateUsername = function(username) {
            return emWamp.call('/user/account#validateUsername', { username: username })
                .then(function(result) {
                    $scope.usernameValid = result;
                }, logError);
        };

        $scope.validateEmail = function(email) {
            return emWamp.call('/user/account#validateEmail', { email: email })
                .then(function(result) {
                    $scope.emailValid = result;
                }, logError);
        };

        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                return new { isValid: true };
            } else {
                return new { isValid: false, error: passwordPolicyError };
            };
        };

        $scope.register = function(email, password, birthDate, currency) {
            return emWamp.call('/user/account#register', {
                    username: email,
                    email: email,
                    alias: email,
                    password: password,
                    birthDate: birthDate,
                    currency: currency,
                    emailVerificationURL: constants.emailVerificationUrl
                })
                .then(function(result) {
                }, logError);
        };

        function getPasswordPolicy() {
            emWamp.call('/user/pwd#getPolicy')
                .then(function(result) {
                        _passwordPolicyRegEx = new RegExp(result.regularExpression);
                        _passwordPolicyError = result.message;
                    },
                    logError);
        };

        function getCountries() {
            emWamp.call('/user/account#getCountries', {
                    expectRegions: false, // true
                    filterByCountry: '',
                    excludeDenyRegistrationCountry: false //true
                })
                .then(function(result) {
                        $scope.currentIpCountry = result.currentIPCountry;
                        $scope.countries = result.countries;
                    },
                    logError);
        };

        function getCurrencies() {
            emWamp.call('/user/account#getCurrencies')
                .then(function(result) {
                        $scope.currencies = result;
                    },
                    logError);
        };

        function logError(error) {
            console.log(error);
        };

        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function init() {
            getPasswordPolicy();
            getCountries();
            getCurrencies();
        };

        init();

        //$scope.$on('$wamp.open', function(event, session) {
        //    console.log('We are connected to the WAMP Router!');
        //});

        //$scope.$on("$wamp.close", function (event, data) {
        //    $scope.reason = data.reason;
        //    $scope.details = data.details;
        //    console.log('We are connected to the WAMP Router!');
        //});
        
        
        //emService.login('ppet', '1q2w3e4r!@#$').then(
        //    function(result) {
        //        console.log("login => ");
        //        console.log(result);

        //        emService.getUserBasicConfig().then(
        //            function(result1) {
        //                console.log("getUserBasicConfig => ");
        //                console.log(result1);

        //            },
        //            function(error) {
        //                console.log(error);
        //            });
        //    },
        //    function(error) {
        //        console.log(error);
        //    });
    }

})();