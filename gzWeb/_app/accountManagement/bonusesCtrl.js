(function () {
    'use strict';
    var ctrlId = 'bonusesCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'auth', 'message', ctrlFactory]);
    function ctrlFactory($scope, constants, auth, message) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.bonusCode = '';
        $scope.submit = function () {
            if ($scope.form.$valid) {
                $scope.applyingBonus = true;
                auth.applyBonus($scope.bonusCode).then(function () {
                    $scope.applyingBonus = false;
                    $scope.bonusCode = '';
                    message.success("Your request for bonus has been registered!");
                }, function (error) {
                    $scope.applyingBonus = false;
                    message.error(error.desc);
                });
            }
        };

        //function init() {
        //    auth.getGrantedBonuses().then(function (result) {
        //        $scope.bonuses = result.bonuses;
        //    }, function (error) {
        //        message.error(error.desc);
        //    });
        //}
        //init();
    }
})();