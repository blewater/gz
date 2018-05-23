(function () {
    "use strict";
    var ctrlId = "gdprCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'auth', 'constants', '$location', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, auth, constants, $location) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.waiting = false;
        $scope.responseMsg = null;

        $scope.consents = {
            allowGzEmail: undefined,
            allowGzSms: undefined,
            allow3rdPartySms: undefined,
            acceptedGdprTc: undefined,
            acceptedGdprPp: undefined,
            acceptedGdpr3rdParties: undefined
        };

        $scope.isFormValid = function () {
            var isValid = true;
            for (var consent in $scope.consents) {
                var consentValue = $scope.consents[consent];
                isValid = isValid && consentValue !== undefined;
            }
            return isValid;
        };

        $scope.logout = function () {
            $scope.logoutWaiting = true;
            auth.logout();
        };

        $scope.submit = function () {
            if ($scope.isFormValid() && !$scope.continueWaiting) {
                $scope.continueWaiting = true;
                $scope.errorMsg = "";

                auth.setGdpr($scope.consents).then(function (response) {
                    $scope.continueCallback();
                    $scope.nsOk();
                });
            }
        };

        window.appInsights.trackPageView("GDPR");
    }
})();