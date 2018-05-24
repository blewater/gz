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
            var consObj = $scope.consents;
            // Everymatrix guideline: user answers once for the 3 consent questions.
            if ( 
                (consObj["acceptedGdprTc"] === undefined || consObj["acceptedGdprTc"] === "false")
                || consObj["allowGzEmail"] === undefined
                || consObj["allowGzSms"] === undefined
                || consObj["allow3rdPartySms"] === undefined) {
                return false;
            }
            return true;
        };

        $scope.logout = function () {
            $scope.logoutWaiting = true;
            auth.logout();
        };

        $scope.submit = function () {
            if ($scope.isFormValid() && !$scope.continueWaiting) {
                // in the popup are 1 question with T&C
                $scope.consents["acceptedGdprPp"] = $scope.consents["acceptedGdpr3rdParties"] = true;

                $scope.continueWaiting = true;
                $scope.errorMsg = "";

                auth.setGdpr($scope.consents).then(function (response) {
                    $scope.nsOk();
                });
            }
        };

        window.appInsights.trackPageView("GDPR");
    }
})();