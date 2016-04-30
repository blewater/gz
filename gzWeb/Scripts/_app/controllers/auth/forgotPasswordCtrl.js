(function () {
    "use strict";
    var ctrlId = "forgotPasswordCtrl";
    APP.controller(ctrlId, ['$scope', 'constants', 'vcRecaptchaService', ctrlFactory]);
    function ctrlFactory($scope, constants, vcRecaptchaService) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;
        $scope.reCaptchaPublicKey = constants.reCaptchaPublicKey;
        $scope.emailValidOnce = false;
        var unregisterIsEmailValidWatch = $scope.$watch(function(){
            return $scope.form.email.$dirty && $scope.form.email.$valid; 
        }, function (newValue, oldValue) {
            if (newValue === true) {
                $scope.emailValidOnce = true;
                unregisterIsEmailValidWatch();
            }
        });

        $scope.model = {
            email: null
        };

        $scope.backToLogin = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: '600px',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        
        $scope.sendInstructions = function () {
            if ($scope.form.$valid)
                sendInstructions();
        };
        function sendInstructions(){
            
        }
    }
})();