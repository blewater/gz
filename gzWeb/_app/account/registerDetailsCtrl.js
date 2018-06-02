﻿(function () {
    'use strict';
    var ctrlId = 'registerDetailsCtrl';
    APP.controller(ctrlId, ['$scope', '$filter', 'emWamp', 'auth', 'message', 'constants', '$log', ctrlFactory]);
    function ctrlFactory($scope, $filter, emWamp, auth, message, constants, $log) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

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
            { title: "Account Details" },
            { title: "Personal Details" },
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
            loadMonths(country, $scope.model.yearOfBirth);

            $scope.loadingYears = false;
        }
        $scope.onYearSelected = function (year) {
            loadMonths($scope.model.country, year);
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
            emWamp.getCountries(false /*true*/, '', true).then(function(result) {
                $scope.currentIpCountry = result.currentIPCountry;
                $scope.countries = result.countries;
                if ($scope.currentIpCountry == null || $scope.currentIpCountry == '' || $scope.currentIpCountry == undefined)
                    $scope.currentIpCountry = 'GR';

                $scope.model.country = $filter('filter')($scope.countries, { code: $scope.currentIpCountry })[0];
                $scope.phonePrefixes = $filter('map')($scope.countries, function(c) {
                    return {
                        country: c.code,
                        code: c.phonePrefix,
                        name: c.name + " (" + c.phonePrefix + ")"
                    };
                });
                if ($scope.model.country)
                    $scope.onCountrySelected($scope.model.country.code);
                $scope.loadingCountries = false;
            }, function(error) {
                $log.error(error.desc);
            });
        };
        $scope.onCountrySelected = function (countryCode) {
            var country = $filter('filter')($scope.countries, { code: countryCode })[0];

            loadYears(country);
            $scope.onPhonePrefixSelected($filter('map')($scope.phonePrefixes, function(x) { return x.country; }).indexOf(country.code));
            if ($scope.currencies.length > 0)
                selectCurrency($scope.currencies, country);
        };
        $scope.onPhonePrefixSelected = function (prefixIndex){
            for (var i = 0; i < $scope.phonePrefixes.length; i++)
                $scope.phonePrefixes[i].active = false;
            $scope.phonePrefixes[prefixIndex].active = true;
            $scope.model.phonePrefix = $scope.phonePrefixes[prefixIndex].code;
        }
        $scope.setDropdownOffset = function(isOpen){
            if (isOpen){
                var $menu = $('#phonePrefix.open > ul.dropdown-menu');
                var $items = $menu.find('li');
                var $active = $menu.find('li.active');
                var index = $items.index($active);
                $menu.scrollTop(index * $active.height());            
            }
        }
        
        $scope.currencies = [];
        function loadCurrencies() {
            $scope.loadingCurrencies = true;
            emWamp.getCurrencies().then(function (result) {
                $scope.currencies = result;
                if ($scope.model.country !== null)
                    selectCurrency($scope.currencies, $scope.model.country);
                $scope.loadingCurrencies = false;
            }, function(error) {
                $log.error(error.desc);
            });
        };

        function getUserConsent() {
            auth.setUserConsentQuestions($scope, 1).then(function() {
                $scope.waiting = false;
            },
            function(errorUserConsent) {
                $scope.waiting = false;
                message.error(errorUserConsent);
            });
        }

        function selectCurrency(currencies, country) {
            var foundCurrencies = $filter('filter')(currencies, { code: country.currency });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')(currencies, { code: 'EUR' })[0];
        }

        function init() {
            $scope.waiting = true;
            getUserConsent();
            loadYears();
            loadMonths();
            loadTitles();
            loadCountries();
            loadCurrencies();
            window.appInsights.trackPageView("REGISTER DETAILS");
        };
        // #endregion

        $scope.checkAddressValidity = function () {
            //$scope.addresIsInvalid = $scope.model.address.split(" ").length < 2;
            $scope.addresIsInvalid = false;
        };

        // #region register
        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting)
                register();
        };
        
        function register() {
            $scope.waiting = true;
            var dateOfBirth = moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth.value - 1, $scope.model.dayOfBirth.value]);
            var gender = $filter('filter')($filter('toArray')(titles), { display: $scope.model.title })[0].gender;
            var parameters = {
                username: $scope.model.username,
                email: $scope.model.email,
                alias: $scope.model.username,
                password: $scope.model.password,
                firstname: $scope.model.firstname,
                lastname: $scope.model.lastname,
                dateOfBirth: dateOfBirth,
                dateOfBirthFormatted: dateOfBirth.format('YYYY-MM-DD'),
                country: $scope.model.country.code,
                // TODO: region 
                // TODO: personalID 
                mobilePrefix: $scope.model.phonePrefix,
                mobile: $scope.model.phoneNumber,
                currency: $scope.model.currency.code,
                title: $scope.model.title,
                gender: gender,
                city: $scope.model.city,
                address1: $scope.model.address,
                address2: '',
                postalCode: $scope.model.postalCode,
                language: 'en',
                emailVerificationURL: "http://localhost/activate/",
                securityQuestion: "(empty security question)",
                securityAnswer: "(empty security answer)"
            };
            parameters["userConsents"] = auth.createUserConsents($scope);
            auth.register(parameters).then(function () {
                $scope.waiting = false;
                $scope.nsNext({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '_app/account/registerPaymentMethods.html',
                    nsCtrl: 'registerPaymentMethodsCtrl',
                    nsStatic: true,
                    nsParams: { accountModel: $scope.model }
                });
                auth.setGdpr($scope.consents);
            }, function(error) {
                $scope.waiting = false;
                message.error(error);
            });
        };
        // #endregion

        // #region terms and conditions
        $scope.consents = {
            // hasToAcceptTC not set in registration
            isTc : false,
            // always set UserConsent in registration
            isUc : true,
            allowGzEmail: undefined,
            allowGzSms: undefined,
            allow3rdPartySms: undefined,
            acceptedGdprTc: undefined,
            acceptedGdprPp: undefined,
            acceptedGdpr3rdParties: undefined
        };

        $scope.isGdprFormSectionValid = function() {
            if ($scope.waiting)
                return false;
            return $scope.model.acceptedInvTc === "true" && auth.isGdprFormSectionValid($scope);
        }

        $scope.readGamesTerms = function(){
            message.modal("Gaming terms and conditions", {
                nsSize: 'xl',
                nsTemplate: '_app/guest/termsConfirm.html',
                nsParams: { isGaming: true }
            });
        };
        $scope.readInvestmentTerms = function () {
            message.modal("Investment terms and conditions", {
                nsSize: 'xl',
                nsTemplate: '_app/guest/termsConfirm.html',
                nsParams: { isInvestment: true }
            });
        };
        $scope.readPrivacyPolicy = function () {
            message.modal("Privacy Policy", {
                nsSize: 'xl',
                nsTemplate: '_app/guest/termsConfirm.html',
                nsParams: { isPrivacy: true }
            });
        };
        $scope.readCookiePolicy = function () {
            message.modal("Cookie Policy", {
                nsSize: 'xl',
                nsTemplate: '_app/guest/termsConfirm.html',
                nsParams: { isCookie: true }
            });
        };
        // #endregion

        init();
    }
})();