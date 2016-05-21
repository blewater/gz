(function () {
    'use strict';
    var ctrlId = 'registerCtrl';
    APP.controller(ctrlId, ["$scope", "$state", "$http", "$filter", "emWamp", "api", "constants", ctrlFactory]);
    function ctrlFactory($scope, $state, $http, $filter, emWamp, api, constants) {
        $scope.model = {
            email: null,
            username: null,
            password: null,
            firstname: null,
            lastname: null,
            yearOfBirth: null,
            monthOfBirth: null,
            dayOfBirth: null,
            title: 'Mr.',
            address: null,
            postalCode: null,
            city: null,
            currency: null,
            country: null,
            phonePrefix: null,
            phoneNumber: null
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

        $scope.updateDaysOfMonth = function(year, month) {
            var daysInMonth = moment.utc([year, month-1]).daysInMonth();
            $scope.daysOfMonth.slice(0, $scope.daysOfMonth.length);
            for (var m = 1; m <= daysInMonth; m++) {
                var pad = '00';
                var str = '' + m;
                $scope.daysOfMonth.push({
                    value: m,
                    display: ('00' + m).substring(0, pad.length - str.length) + str
                });
            };
        };

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
                return emWamp.validateEmail(email)
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
                return emWamp.validateUsername(username)
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
            //if ($scope.form.$valid) {
            //    //$scope.register();
            //}
            if ($scope.userForm.$valid) {
                $state.go('form.details');
            } else {
                return false;
            }

            return true;
        };
        
        $scope.onCountryChanged = function () {
            $scope.phonePrefixes.splice(0, $scope.phonePrefixes.length);
            $scope.phonePrefixes.push($scope.model.country.phonePrefix);
            var foundCurrencies = $filter('filter')($scope.currencies, { code: $scope.model.country.currency });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')($scope.currencies, { code: 'USD' })[0];
        };
        
        $scope.register = function () {

            var emRegisterQ = emRegister("(empty callbackUrl)");

            emRegisterQ.then(function(result) {
                emWamp.login({ usernameOrEmail: $scope.model.username, password: $scope.model.password })
                    .then(function(result) {
                        gzRegister().then(function(result) {
                            //api.login($scope.model.username, $scope.model.password)
                            //    .then(function(result) {
                            //        // TODO: inform user...
                            //    }, logError);
                        }, logError);
                    }, logError);
            }, logError);
        };

        function emRegister(callbackUrl) {

            return emWamp.register({
                username: $scope.model.username,
                email: $scope.model.email,
                alias: $scope.model.username,
                password: $scope.model.password,
                firstname: $scope.model.firstname,
                surname: $scope.model.lastname,
                birthDate: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth - 1, $scope.model.dayOfBirth]).format('YYYY-MM-DD'),
                country: $scope.model.country.code,
                // TOOD: region 
                // TOOD: personalID 
                mobilePrefix: $scope.model.phonePrefix,
                mobile: $scope.model.phoneNumber,
                currency: $scope.model.currency.code,
                title: $scope.model.title,
                gender: "M",
                city: $scope.model.city,
                address1: $scope.model.address,
                address2: '',
                postalCode: $scope.model.postalCode,
                language: 'en',
                emailVerificationURL: callbackUrl,
                securityQuestion: "(empty security question)",
                securityAnswer: "(empty security answer)"
            });
        }

        function gzRegister() {
            return $http.post('/api/Account/Register', {
                Username: $scope.model.username,
                Email: $scope.model.email,
                Password: $scope.model.password,
                FirstName: $scope.model.firstname,
                LastName: $scope.model.lastname,
                Birthday: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth - 1, $scope.model.dayOfBirth]),
                Currency: $scope.model.currency.code,
                Title: $scope.model.title,
                Country: $scope.model.country.code,
                MobilePrefix: $scope.model.phonePrefix,
                Mobile: $scope.model.phoneNumber,
                City: $scope.model.city,
                Address: $scope.model.address,
                PostalCode: $scope.model.postalCode,
                // TODO: Region ???
            });
        }

        function getPasswordPolicy() {
            emWamp.getPasswordPolicy()
                .then(function(result) {
                        _passwordPolicyRegEx = new RegExp(result.regularExpression);
                        _passwordPolicyError = result.message;
                    },
                    logError);
        };

        function getCountries() {
            emWamp.getCountries(false /*true*/, "", false /*true*/)
                .then(function(result) {
                        $scope.currentIpCountry = result.currentIPCountry;
                        $scope.countries = result.countries;
                        if ($scope.currentIpCountry == null || $scope.currentIpCountry == '' || $scope.currentIpCountry == undefined)
                            $scope.currentIpCountry = 'GR';

                        $scope.model.country = $filter('filter')($scope.countries, { code: $scope.currentIpCountry })[0];
                        $scope.onCountryChanged();
                    },
                    logError);
        };

        function getCurrencies() {
            emWamp.getCurrencies()
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
            var year = moment().year();
            do {
                $scope.years.push(year);
                year--;
                maxYear--;
            } while (maxYear > 0);
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