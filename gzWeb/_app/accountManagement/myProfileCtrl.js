(function () {
    'use strict';
    var ctrlId = 'myProfileCtrl';
    APP.controller(ctrlId, ['$scope', 'emWamp', '$filter', 'message', 'constants', ctrlFactory]);
    function ctrlFactory($scope, emWamp, $filter, message, constants) {
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
            currency: null,
            acceptNewsEmail: true,
            acceptSMSOffer: true,
        }

        var titles = {
            mr: { display: 'Mr.', gender: 'M' },
            ms: { display: 'Ms.', gender: 'F' },
            mrs: { display: 'Mrs.', gender: 'F' },
            miss: { display: 'Miss', gender: 'F' }
        };

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
            $scope.titles = $filter('map')($filter('toArray')(titles), function (t) { return t.display; });
            $scope.loadingTitles = false;
        }

        $scope.currentIpCountry = '';
        $scope.countries = [];
        $scope.phonePrefixes = [];
        function loadCountries() {
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
            }, function (error) {
                console.log(error.desc);
            });
        };
        $scope.onCountrySelected = function (countryCode) {
            var country = $filter('filter')($scope.countries, { code: countryCode })[0];

            loadYears(country);
            $scope.onPhonePrefixSelected($filter('map')($scope.phonePrefixes, function (x) { return x.country; }).indexOf(country.code));
            if ($scope.currencies.length > 0)
                selectCurrency($scope.currencies, country);
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
        function loadCurrencies() {
            $scope.loadingCurrencies = true;
            emWamp.getCurrencies().then(function (result) {
                $scope.currencies = result;
                if ($scope.model.country !== null)
                    selectCurrency($scope.currencies, $scope.model.country);
                $scope.loadingCurrencies = false;
            }, function (error) {
                console.log(error.desc);
            });
        };

        function selectCurrency(currencies, country) {
            var foundCurrencies = $filter('filter')(currencies, { code: country.currency });
            $scope.model.currency = foundCurrencies.length > 0
                ? foundCurrencies[0]
                : $filter('filter')(currencies, { code: 'USD' })[0];
        }

        function getProfile() {
            emWamp.getProfile().then(function (response) {
                $scope.profile = response;

                $scope.model.email = $scope.profile.fields.email;
                $scope.model.username = $scope.profile.fields.username;
                $scope.model.userId = $scope.profile.fields.userID;
                $scope.model.yearOfBirth = $scope.profile.fields.birthYear;
                $scope.model.monthOfBirth = { value: $scope.profile.fields.birthMonth, display: $filter('pad')($scope.profile.fields.birthMonth, 2) };
                $scope.model.dayOfBirth = { value: $scope.profile.fields.birthDay, display: $filter('pad')($scope.profile.fields.birthDay, 2) };
                $scope.onYearSelected($scope.model.yearOfBirth);
                $scope.onMonthSelected($scope.model.monthOfBirth);
                $scope.model.gender = $scope.profile.fields.gender;
                $scope.model.title = $scope.profile.fields.title;
                $scope.model.firstname = $scope.profile.fields.firstname;
                $scope.model.surname = $scope.profile.fields.surname;
                $scope.model.phonePrefix = $scope.profile.fields.mobilePrefix;
                $scope.model.phoneNumber = $scope.profile.fields.mobile;
                $scope.model.country = $scope.profile.fields.country;
                $scope.model.address = $scope.profile.fields.address1;
                $scope.model.city = $scope.profile.fields.city;
                $scope.model.postalCode = $scope.profile.fields.postalCode;
                $scope.model.currency = $scope.profile.fields.currency;
                $scope.model.acceptNewsEmail = $scope.profile.fields.acceptNewsEmail;
                $scope.model.acceptSMSOffer = $scope.profile.fields.acceptSMSOffer;
            });
        }

        function init() {
            loadYears();
            loadMonths();
            loadTitles();
            loadCountries();
            loadCurrencies();
            getProfile();
        };

        init();
        // #endregion

        $scope.submit = function () {
            if ($scope.form.$valid) {
                $scope.waiting = true;
                var dateOfBirth = moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth.value - 1, $scope.model.dayOfBirth.value]);
                var gender = $filter('filter')($filter('toArray')(titles), { display: $scope.model.title })[0].gender;
                var parameters = {
                    gender: gender,
                    title: $scope.model.title,
                    firstname: $scope.model.firstname,
                    surname: $scope.model.lastname,
                    birthDate: dateOfBirth,
                    mobilePrefix: $scope.model.phonePrefix,
                    mobile: $scope.model.phoneNumber,
                    country: $scope.model.country.code,
                    address1: $scope.model.address,
                    city: $scope.model.city,
                    postalCode: $scope.model.postalCode,
                    currency: $scope.model.currency.code,
                    acceptNewsEmail: $scope.model.acceptNewsEmail,
                    acceptSMSOffer: $scope.model.acceptSMSOffer
                };
                emWamp.updateProfile(parameters).then(function () {
                    $scope.waiting = false;
                    message.success("Your profile has been updated successfully!", { nsType: 'toastr' });
                }, function (error) {
                    $scope.waiting = false;
                    message.error(error);
                });
            }
        };
    }
})();