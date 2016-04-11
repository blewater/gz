(function () {
    'use strict';
    var ctrlId = 'registerCtrl';
    APP.controller(ctrlId, ['$scope', '$state', 'emWamp', 'constants', ctrlFactory]);
    function ctrlFactory($scope, $state, emWamp, constants) {
        $scope.model = {
            email: null,
            username: null,
            password: null,
            firstname: null,
            lastname: null,
            yearOfBirth: null,
            monthOfBirth: 1,
            dayOfBirth: 1,
            title: 'Mr.',
            postalCode: null,
            city: null,
            currency: null,
            country: null,
            phobePrefix: null,
            phobeNumber: null
        };

        $scope.years = [];
        $scope.months = [
            { value: 1, display: '01' },
            { value: 2, display: '02' },
            { value: 3, display: '03' },
            { value: 4, display: '04' },
            { value: 5, display: '05' },
            { value: 6, display: '06' },
            { value: 7, display: '07' },
            { value: 8, display: '08' },
            { value: 9, display: '09' },
            { value: 10, display: '10' },
            { value: 11, display: '11' },
            { value: 12, display: '12' }
        ];
        $scope.daysOfMonth = [];
        $scope.phonePrefixes = [];
        $scope.titles = [
            'Mr.',
            'Ms.',
            'Mrs.',
            'Miss'
        ];

        $scope.usernameValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.emailValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.passwordValidation = {
            //isValidating: false,
            isValid: true,
            error: ''
        };

        $scope.currentIpCountry = '';
        $scope.countries = [];
        $scope.currencies = [];

        $scope.resetEmailValidation = function () {
            $scope.emailValidation.isValidating = false;
            $scope.emailValidation.isAvailable = true;
            $scope.emailValidation.error = '';
        };
        $scope.validateEmail = function (email) {
            if (!$scope.emailValidation.isValidating) {
                $scope.emailValidation.isValidating = true;
                return emWamp.call('/user/account#validateEmail', { email: email })
                    .then(function (result) {
                        $scope.emailValidation.isValidating = false;
                        $scope.emailValidation.isAvailable = result.isAvailable;
                        $scope.emailValidation.error = result.error;
                    }, function (error) {
                        $scope.emailValidation.isValidating = false;
                        logError(error);
                    });
            }
        };

        $scope.resetUsernameValidation = function () {
            $scope.usernameValidation.isValidating = false;
            $scope.usernameValidation.isAvailable = true;
            $scope.usernameValidation.error = '';
        };
        $scope.validateUsername = function (username) {
            if (!$scope.usernameValidation.isValidating) {
                return emWamp.call('/user/account#validateUsername', { username: username })
                    .then(function (result) {
                        $scope.usernameValidation.isValidating = false;
                        $scope.usernameValidation.isAvailable = result.isAvailable;
                        $scope.usernameValidation.error = result.error;
                    }, function (error) {
                        $scope.usernameValidation.isValidating = false;
                        logError(error);
                    });
            }
        };

        $scope.resetPasswordValidation = function() {
            $scope.passwordValidation.isValid = true;
            $scope.passwordValidation.error = '';
        };
        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                $scope.passwordValidation.isValid = true;
            } else {
                $scope.passwordValidation.isValid = false;
                $scope.passwordValidation.error = _passwordPolicyError;
            };
        };

        $scope.proccedToUserDetails = function () {

            if ($scope.userForm.$valid) {
                $state.go('form.details');
            } else {
                return false;
            }

            return true;
        };

        $scope.onCountryChanged = function () {
            var phonePrefix = '';
            angular.forEach($scope.countries, function(value) {
                if (value.code == $scope.model.country)
                    phonePrefix = value.phonePrefix;
            });
            $scope.phonePrefixes.splice(0, $scope.phonePrefixes.length);
            $scope.phonePrefixes.push(phonePrefix);
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

            var maxYear = 100;
            var year = new Date().getFullYear() - 18;
            $scope.yearOfBirth = year;
            do {
                $scope.years.push(year);
                //$angular.array($scope.years, year);
                year--;
                maxYear--;
            } while (maxYear > 0);

            for (var m = 1; m < 32; m++) {
                $scope.daysOfMonth.push(m);
            };

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