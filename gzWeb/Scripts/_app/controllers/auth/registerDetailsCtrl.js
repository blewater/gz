(function () {
    'use strict';
    var ctrlId = 'registerDetailsCtrl';
    APP.controller(ctrlId, ['$scope', '$rootScope', '$http', '$filter', 'emWamp', 'message', 'api', ctrlFactory]);
    function ctrlFactory($scope, $rootScope, $http, $filter, emWamp, message, api) {
        var titles = {
            mr: { display: 'Mr.', gender: 'M' },
            ms: { display: 'Ms.', gender: 'F' },
            mrs: { display: 'Mrs.', gender: 'F' },
            miss: { display: 'Miss', gender: 'F' }
        };
        $scope.model = {
            email: $scope.startModel.email,
            username: $scope.startModel.username,
            password: $scope.startModel.password,

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
            phoneNumber: null,
            agreed: false
        };
        
        // #region steps
        $scope.currentStep = 1;
        $scope.steps = [
            { title: "Create Account" },
            { title: "Enter Details" },
            { title: "Deposit" }
        ];
        // #endregion

        // #region init
        function loadYears(country) {
            $scope.loadingYears = true;

            var yearOfBirth = $scope.model.yearOfBirth;
            var thisYear = moment().year();
            var maxYear = country ? thisYear - country.legalAge : thisYear;
            $scope.years = $filter('getRange')([], maxYear, 1900, 1, true);
            if ($scope.years.indexOf(yearOfBirth) !== -1)
                $scope.model.yearOfBirth = yearOfBirth;
            loadMonths(country, $scope.yearOfBirth);

            $scope.loadingYears = false;
        }
        $scope.onYearSelected = function (year) {
            loadMonths($scope.model.country, year);
            //if ($scope.model.monthOfBirth)
            //    loadDays(year, $scope.model.monthOfBirth);
        };

        function loadMonths(country, year) {
            $scope.loadingMonths = true;

            var monthOfBirth = $scope.model.monthOfBirth;
            var thisYear = moment().year();
            var maxYear = country ? thisYear - country.legalAge : thisYear;
            var maxMonth = year === maxYear ? moment().month() : 12;
            $scope.months = $filter('map')(
                $filter('getRange')([], maxMonth, 1),
                function (m) {
                    return {
                        value: m,
                        display: $filter('pad')(m, 2)
                    };
                });
            if (monthOfBirth && $filter('map')($scope.months, function (m) { return m.value; }).indexOf(monthOfBirth.value) !== -1)
                $scope.model.monthOfBirth = monthOfBirth;
            if ($scope.model.monthOfBirth)
                loadDays(country, year, $scope.model.monthOfBirth.value);

            $scope.loadingMonths = false;
        }
        $scope.onMonthSelected = function (month) {
            loadDays($scope.model.country, $scope.model.yearOfBirth, month);
            //if ($scope.model.yearOfBirth)
            //    loadDays($scope.model.yearOfBirth, month);
        };

        $scope.daysOfMonth = [];
        function loadDays(country, year, month) {
            $scope.loadingDays = true;

            var dayOfBirth = $scope.model.dayOfBirth;
            var thisYear = moment().year();
            var maxYear = country ? thisYear - country.legalAge : thisYear;
            var maxMonth = year === maxYear ? moment().month() : 12;
            var daysInMonth = moment.utc([year, month - 1]).daysInMonth();
            var maxDay = month === maxMonth ? moment().date() : daysInMonth;
            
            $scope.daysOfMonth.length = 0;
            for (var m = 1; m <= maxDay; m++) {
                $scope.daysOfMonth.push({
                    value: m,
                    display: $filter('pad')(m, 2)
                });
            };
            if (dayOfBirth && $filter('map')($scope.daysOfMonth, function (d) { return d.value; }).indexOf(dayOfBirth.value) !== -1)
                $scope.model.dayOfBirth = dayOfBirth;

            //if ($scope.daysOfMonth.indexOf(dayOfBirth) !== -1)
            //    $scope.model.dayOfBirth = dayOfBirth;

            $scope.loadingDays = false;
        }

        function loadTitles() {
            $scope.loadingTitles = true;
            $scope.titles = $filter('map')($filter('toArray')(titles), function(t) { return t.display; });
            $scope.loadingTitles = false;
        }

        $scope.currentIpCountry = '';
        $scope.countries = [];
        $scope.phonePrefixes = [];
        function loadCountries() {
            $scope.loadingCountries = true;
            emWamp.call('/user/account#getCountries', {
                expectRegions: false, // true
                filterByCountry: '',
                excludeDenyRegistrationCountry: false //true
            })
            .then(function(result) {
                $scope.currentIpCountry = result.currentIPCountry;
                $scope.countries = result.countries;
                if ($scope.currentIpCountry == null || $scope.currentIpCountry == '' || $scope.currentIpCountry == undefined)
                    $scope.currentIpCountry = 'GR';

                $scope.model.country = $filter('filter')($scope.countries, { code: $scope.currentIpCountry })[0];
                $scope.phonePrefixes = $filter('map')($scope.countries, function(c) {
                    return {
                        code: c.phonePrefix,
                        name: c.name + " (" + c.phonePrefix + ")"
                    }
                });
                $scope.onCountrySelected($scope.currentIpCountry);
                $scope.loadingCountries = false;
            }, logError);
        };
        $scope.onCountrySelected = function (countryCode) {
            var country = $filter('filter')($scope.countries, { code: countryCode })[0];

            loadYears(country);
            //if ($scope.phonePrefixes.indexOf(country.phonePrefix) === -1)
            //    $scope.phonePrefixes.push(country.phonePrefix);
            $scope.model.phonePrefix = country.phonePrefix;
            if ($scope.currencies.length > 0)
                selectCurrency($scope.currencies, country);
        };
        
        $scope.currencies = [];
        function loadCurrencies() {
            $scope.loadingCurrencies = true;
            emWamp.call('/user/account#getCurrencies')
                .then(function (result) {
                    $scope.currencies = result;
                    if ($scope.model.country !== null)
                        selectCurrency($scope.currencies, $scope.model.country);
                    $scope.loadingCurrencies = false;
                }, logError);
        };

        function selectCurrency(currencies, country) {
            var foundCurrencies = $filter('filter')(currencies, { code: country.currency });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')(currencies, { code: 'USD' })[0];
        }

        function init() {
            loadYears();
            loadMonths();
            loadTitles();
            loadCountries();
            loadCurrencies();
        };
        // #endregion

        // #region register
        $scope.submit = function () {
            if ($scope.form.$valid)
                register();
        };
        
        function register() {
            var emPromise = emRegister("http://localhost/activate/");
            emPromise.then(function(emRegisterResult) {
                emWamp.login({ usernameOrEmail: $scope.model.username, password: $scope.model.password }).then(function (emLoginResult) {
                    gzRegister().then(function(gzRegisterResult) {
                        api.login($scope.model.username, $scope.model.password).then(function (gzLoginResult) {
                            $scope.nsOk($scope.model);
                        }, logError);
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
                birthDate: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth.value - 1, $scope.model.dayOfBirth.value]).format('YYYY-MM-DD'),
                country: $scope.model.country.code,
                // TOOD: region 
                // TOOD: personalID 
                mobilePrefix: $scope.model.phonePrefix,
                mobile: $scope.model.phoneNumber,
                currency: $scope.model.currency.code,
                title: $scope.model.title,
                gender: $filter('filter')($filter('toArray')(titles), { display: $scope.model.title })[0].gender,
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
                Birthday: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth.value - 1, $scope.model.dayOfBirth.value]),
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
        // #endregion

        function logError(error) {
            console.log(error);
        };

        // #region Terms and conditions
        $scope.readTerms = function(){
            var promise = message.modal("Terms and conditions", {
                nsSize: 'xl',
                nsTemplate: '/partials/messages/terms.html'
            });
            promise.then(function() {
                $scope.model.agreed = true;
            }, function() {
                $scope.model.agreed = false;
            });
        };
        // #endregion

        init();
    }
})();