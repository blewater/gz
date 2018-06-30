(function () {
    "use strict";
    var ctrlId = "gdprCtrl";
    APP.controller(ctrlId, ['$rootScope', '$scope', 'auth', 'constants', ctrlFactory]);
    function ctrlFactory($rootScope, $scope, auth, constants) {

        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.waiting = false;
        $scope.responseMsg = null;

        // UserConsentList Api Constants
        $scope.tcApiCode = "termsandconditions";
        $scope.emailApiCode = "emailmarketing";
        $scope.smsApiCode = "sms";
        $scope.thirdpartyApiCode = "3rdparty";

        // T&C may be set by getConsentRequirements or hasToAcceptTC
        $scope.showTcbyUserConsentApi = false;
        $scope.showEmail = false;
        $scope.showSms = false;
        $scope.show3rdParty = false;

        function init() {
            $scope.waiting = true;
            auth.setUserConsentQuestions($scope, 2).then(function() {
                $scope.waiting = false;
            },
            function(errorUserConsent) {
                $scope.waiting = false;
                message.warning(errorUserConsent);
            });
        }
        init();

        $scope.consents = {
            // hasToAcceptTC
            isTc : $scope.isTc,
            // hasToSetUserConsent
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

            // TC by either accept or userconsent Api
            if ($scope.showTcbyUserConsentApi && acceptedGdprTc === undefined || acceptedGdprTc === "false")
                return false;

            // marketing consent
            if ($scope.isUc)
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
                $scope.continueWaiting = true;
                $scope.errorMsg = "";

                $scope.consents.isTc = $scope.isTc || $scope.showTcbyUserConsentApi;
                // Assume if the tc question was asked, it must be accepted by the user
                if ($scope.showTcbyUserConsentApi) {
                    $scope.consents.acceptedGdprPp = true;
                    $scope.consents.acceptedGdpr3rdParties = true;
                }

                auth.setGdpr($scope.consents).then(function (response) {
                    $scope.nsOk({
                        setAcceptTC : $scope.isTc, 
                        setUserConsent : $scope.isUc,
                        userConsents: auth.createUserConsents($scope)
                    });
                });
            }
        };

        window.appInsights.trackPageView("GDPR");
    }
})();