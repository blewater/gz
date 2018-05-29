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
            isTc : $scope.isTc,
            isUc : $scope.isUc,
            allowGzEmail: undefined,
            allowGzSms: undefined,
            allow3rdPartySms: undefined,
            acceptedGdprTc: undefined,
            acceptedGdprPp: undefined,
            acceptedGdpr3rdParties: undefined
        };

        $scope.isFormValid = function () {
            var consObj = $scope.consents;
            var acceptedGdprTc = consObj.acceptedGdprTc;
            var isTc = $scope.isTc;
            if (isTc && (acceptedGdprTc === undefined || acceptedGdprTc === "false")) {
                return false;
            }
            // Everymatrix guideline: user answers once for the 3 consent questions.
            if (!isTc)
                if  (consObj.allowGzEmail === undefined
                    || consObj.allowGzSms === undefined
                    || consObj.allow3rdPartySms === undefined) {
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
                $scope.consents.isTc = $scope.isTc;
                // in the popup there is 1 question with T&C
                $scope.consents.acceptedGdprPp = $scope.consents.acceptedGdpr3rdParties = true;

                $scope.continueWaiting = true;
                $scope.errorMsg = "";

                auth.setGdpr($scope.consents).then(function (response) {
                    $scope.nsOk(true);
                });
            }
        };

        window.appInsights.trackPageView("GDPR");
    }
})();