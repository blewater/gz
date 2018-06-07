(function () {
    'use strict';
    var ctrlId = 'myProfileCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', '$filter', 'message', 'constants', '$q', '$log', '$timeout', 'auth', ctrlFactory]);
    function ctrlFactory($scope, emWamp, $filter, message, constants, $q, $log, $timeout, auth) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        // #region Init
        $scope.model = {
            email: null,
            username: null,
            userId: null,

            yearOfBirth: null,
            monthOfBirth: null,
            dayOfBirth: null,
            gender: null,
            title: null,
            firstname: null,
            surname: null,
            mobilePrefix: null,
            mobile: null,
            country: null,
            address1: null,
            city: null,
            postalCode: null,
            currency: null
        }

        var titles = {
            mr: { display: 'Mr.', gender: 'M' },
            ms: { display: 'Ms.', gender: 'F' },
            mrs: { display: 'Mrs.', gender: 'F' },
            miss: { display: 'Miss', gender: 'F' }
        };

        // #region terms and conditions
        $scope.consents = {
            // hasToAcceptTC not set in profile
            isTc : false,
            // always set UserConsent in profile
            isUc : true,
            allowGzEmail: undefined,
            allowGzSms: undefined,
            allow3rdPartySms: undefined,
            acceptedGdprTc: true,
            acceptedGdprPp: undefined,
            acceptedGdpr3rdParties: undefined
        };

        function loadYears(country, defer) {
            $scope.loadingYears = true;

            var yearOfBirth = $scope.model.yearOfBirth;
            var thisYear = moment().year();
            var maxYear = country ? thisYear - country.legalAge : thisYear;
            $scope.years = $filter('getRange')([], maxYear, 1900, 1, true);
            if ($scope.years.indexOf(yearOfBirth) !== -1)
                $scope.model.yearOfBirth = yearOfBirth;
            loadMonths(country, $scope.yearOfBirth);

            $scope.loadingYears = false;

            if (defer)
                defer.resolve(true);
        }
        $scope.onYearSelected = function (year) {
            loadMonths($scope.model.country, year);
        };

        function loadMonths(country, year, defer) {
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

            if (defer)
                defer.resolve(true);
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
            var daysInMonth = moment.utc([year, month.value - 1]).daysInMonth();
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

        function loadTitles(defer) {
            $scope.loadingTitles = true;
            $scope.titles = $filter('map')($filter('toArray')(titles), function (t) { return t.display; });
            $scope.loadingTitles = false;
            defer.resolve(true);
        }

        $scope.currentIpCountry = '';
        $scope.countries = [];
        $scope.phonePrefixes = [];
        function loadCountries(defer) {
            $scope.loadingCountries = true;
            emWamp.getCountries(false /*true*/, '', false /*true*/).then(function (result) {
                $scope.currentIpCountry = result.currentIPCountry;
                $scope.countries = result.countries;
                if ($scope.currentIpCountry == null || $scope.currentIpCountry == '' || $scope.currentIpCountry == undefined)
                    $scope.currentIpCountry = 'GR';

                $scope.model.country = $filter('filter')($scope.countries, { code: $scope.currentIpCountry })[0];
                $scope.phonePrefixes = $filter('map')($scope.countries, function (c) {
                    return {
                        country: c.code,
                        code: c.phonePrefix,
                        name: c.name + " (" + c.phonePrefix + ")"
                    };
                });
                $scope.onCountrySelected($scope.currentIpCountry);
                $scope.loadingCountries = false;
                defer.resolve(true);
            }, function (error) {
                $log.error(error.desc);
                defer.resolve(false);
            });
        };
        $scope.onCountrySelected = function (countryCode) {
            var country = $filter('filter')($scope.countries, { code: countryCode })[0];

            loadYears(country);
            $scope.onPhonePrefixSelected($filter('map')($scope.phonePrefixes, function (x) { return x.country; }).indexOf(countryCode));
            if ($scope.currencies.length > 0)
                selectCurrencyByCountry($scope.currencies, country);
        };
        $scope.onPhonePrefixSelected = function (prefixIndex) {
            for (var i = 0; i < $scope.phonePrefixes.length; i++)
                $scope.phonePrefixes[i].active = false;
            $scope.phonePrefixes[prefixIndex].active = true;
            $scope.model.phonePrefix = $scope.phonePrefixes[prefixIndex].code;
        }
        $scope.setDropdownOffset = function (isOpen) {
            if (isOpen) {
                var $menu = $('#phonePrefix.open > ul.dropdown-menu');
                var $items = $menu.find('li');
                var $active = $menu.find('li.active');
                var index = $items.index($active);
                $menu.scrollTop(index * $active.height());
            }
        }

        $scope.currencies = [];
        function loadCurrencies(defer) {
            $scope.loadingCurrencies = true;
            emWamp.getCurrencies().then(function (result) {
                $scope.currencies = result;
                if ($scope.model.country !== null)
                    selectCurrencyByCountry($scope.currencies, $scope.model.country);
                $scope.loadingCurrencies = false;
                defer.resolve(true);
            }, function (error) {
                defer.resolve(false);
                $log.error(error.desc);
            });
        };

        function selectCurrencyByCountry(currencies, country) {
            var foundCurrencies = $filter('filter')(currencies, { code: country.currency });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')(currencies, { code: 'USD' })[0];
        }
        function selectCurrencyByCode(currencies, currencyCode) {
            var foundCurrencies = $filter('filter')(currencies, { code: currencyCode });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')(currencies, { code: 'USD' })[0];
        }

        function getProfile() {
            emWamp.getProfile().then(function (response) {
                $scope.profile = response;

                //$timeout(function () {
                    $scope.model.email = $scope.profile.fields.email;
                    $scope.model.username = $scope.profile.fields.username;
                    $scope.model.userId = $scope.profile.fields.userID;
                    $scope.model.yearOfBirth = $scope.profile.fields.birthYear;
                    //$scope.onYearSelected($scope.model.yearOfBirth);
                    $scope.model.monthOfBirth = { value: $scope.profile.fields.birthMonth, display: $filter('pad')($scope.profile.fields.birthMonth, 2) };
                    $scope.model.dayOfBirth = { value: $scope.profile.fields.birthDay, display: $filter('pad')($scope.profile.fields.birthDay, 2) };
                    $scope.model.gender = $scope.profile.fields.gender;
                    $scope.model.title = $scope.profile.fields.title;
                    $scope.model.firstname = $scope.profile.fields.firstname;
                    $scope.model.surname = $scope.profile.fields.surname;
                    $scope.model.phonePrefix = $scope.profile.fields.mobilePrefix;
                    $scope.model.phoneNumber = $scope.profile.fields.mobile;
                    $scope.model.country = $filter('filter')($scope.countries, { code: $scope.profile.fields.country })[0];
                    $scope.model.address = $scope.profile.fields.address1;
                    $scope.model.city = $scope.profile.fields.city;
                    $scope.model.postalCode = $scope.profile.fields.postalCode;
                    $scope.model.currency = $filter('filter')($scope.currencies, { code: $scope.profile.fields.currency })[0];
                if ($scope.showTcbyUserConsentApi) {
                    var tcConsentVal = $scope.profile.fields.userConsents[constants.emUserConsentKeys.tcApiCode];
                    if (angular.isDefined(tcConsentVal)) {
                        $scope.consents.acceptedGdprTc = tcConsentVal;
                    }
                }
                if ($scope.showEmail) {
                    $scope.consents.allowGzEmail = $scope.profile.fields.acceptNewsEmail || $scope.profile.fields.userConsents[constants.emUserConsentKeys.emailApiCode];
                }
                if ($scope.showSms) {
                    $scope.consents.allowGzSms = $scope.profile.fields.acceptSMSOffer || $scope.profile.fields.userConsents[constants.emUserConsentKeys.smsApiCode];
                }
                if ($scope.show3rdParty) {
                    var thirdPartyConsentVal = $scope.profile.fields.userConsents[constants.emUserConsentKeys.thirdpartyApiCode];
                    if (angular.isDefined(thirdPartyConsentVal)) {
                        $scope.consents.allow3rdPartySms = thirdPartyConsentVal;
                    }
                }

                $scope.onCountrySelected($scope.model.country.code);
                    $scope.onYearSelected($scope.model.yearOfBirth);
                    $scope.onMonthSelected($scope.model.monthOfBirth);
                //}, 0);
            });
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


        function init() {
            //var loadYearsDefer = $q.defer();
            //var loadMonthsDefer = $q.defer();
            //var loadTitlesDefer = $q.defer();
            var loadCountriesDefer = $q.defer();
            var loadCurrenciesDefer = $q.defer();

            //loadYears(null, loadYearsDefer);
            //loadMonths(null, null, loadMonthsDefer);
            //loadTitles(loadTitlesDefer);
            loadCountries(loadCountriesDefer);
            loadCurrencies(loadCurrenciesDefer);

            //loadYearsDefer.promise, loadMonthsDefer.promise, loadTitlesDefer.promise,
            $q.all([auth.setUserConsentQuestions($scope, 3), loadCountriesDefer.promise, loadCurrenciesDefer.promise]).then(function () {
                getProfile();
            });

            //loadYears();
            //loadMonths();
            //loadTitles();
            //loadCountries();
            //loadCurrencies();
            //getProfile();
        };

        init();
        // #endregion

        $scope.submit = function () {
            if ($scope.form.$valid && !$scope.waiting) {
                $scope.waiting = true;
                auth.setGdpr($scope.consents);
                var dateOfBirth = moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth.value - 1, $scope.model.dayOfBirth.value]).format('YYYY-MM-DD');
                var gender = $filter('filter')($filter('toArray')(titles), { display: $scope.model.title })[0].gender;
                var parameters = {
                    email: $scope.model.email,
                    gender: gender,
                    title: $scope.model.title,
                    firstname: $scope.model.firstname,
                    surname: $scope.model.surname,
                    birthDate: dateOfBirth,
                    mobilePrefix: $scope.model.phonePrefix,
                    mobile: $scope.model.phoneNumber,
                    country: $scope.model.country.code,
                    address1: $scope.model.address,
                    city: $scope.model.city,
                    postalCode: $scope.model.postalCode,
                    currency: $scope.model.currency.code,
                    acceptNewsEmail: $scope.consents.allowGzEmail,
                    acceptSMSOffer: $scope.consents.allowGzSms
                };
                parameters["userConsents"] = auth.createUserConsents($scope);
                emWamp.updateProfile(parameters).then(function () {
                    $scope.waiting = false;
                    message.success("Your profile has been updated successfully!", { nsType: 'toastr' });
                }, function (error) {
                    $scope.waiting = false;
                    message.error(error.desc);
                });
            }
        };
    }
})();