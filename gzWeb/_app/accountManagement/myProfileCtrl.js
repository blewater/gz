(function () {
    'use strict';
    var ctrlId = 'myProfileCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
        $scope.gender = ['Male', 'Female'];
        $scope.phonePrefixes = [];

        function loadPhonePrefixes() {
            $scope.phonePrefixes = $filter('map')($scope.countries, function (c) {
                return {
                    country: c.code,
                    code: c.phonePrefix,
                    name: c.name + " (" + c.phonePrefix + ")"
                };
            });
        };

        $scope.onCountrySelected = function (countryCode) {
            var country = $filter('filter')($scope.countries, { code: countryCode })[0];

            loadYears(country);
            $scope.onPhonePrefixSelected($filter('map')($scope.phonePrefixes, function (x) { return x.country; }).indexOf(country.code));
            if ($scope.currencies.length > 0)
                selectCurrency($scope.currencies, country);
        };

    }
})();