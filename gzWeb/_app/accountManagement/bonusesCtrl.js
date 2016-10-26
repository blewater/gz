(function () {
    'use strict';
    var ctrlId = 'bonusesCtrl';
    APP.controller(ctrlId, ['$scope', 'constants', 'auth', 'message', '$filter', ctrlFactory]);
    function ctrlFactory($scope, constants, auth, message, $filter) {
        $scope.spinnerGreen = constants.spinners.sm_rel_green;
        $scope.spinnerWhite = constants.spinners.sm_rel_white;

        $scope.bonusCode = '';
        $scope.submit = function () {
            if ($scope.form.$valid) {
                $scope.applyingBonus = true;
                auth.applyBonus($scope.bonusCode).then(function () {
                    $scope.applyingBonus = false;
                    $scope.bonusCode = '';
                    init();
                    //message.success("Your request for bonus has been registered!");
                }, function (error) {
                    $scope.applyingBonus = false;
                    message.error(error.desc);
                });
            }
        };

        function init() {
            fetchBonuses();
        }
        function fetchBonuses() {
            $scope.fetchingBonuses = true;
            auth.getGrantedBonuses().then(function (result) {
                $scope.fetchingBonuses = false;
                $scope.grantedBonuses = result.bonuses;
            }, function (error) {
                $scope.fetchingBonuses = false;
                message.error(error.desc);
            });
        }
        init();

        $scope.forfeit = function (index) {
            message.confirm("Are you sure you want to continue?", function () {
                auth.forfeit($scope.grantedBonuses[index].id).then(function () {
                    $scope.grantedBonuses.splice(index, 1);
                });
            }, angular.noop, {
                nsBody:
                    "You are going to forfeit the selected bonus of " +
                    $filter('isoCurrency')($scope.grantedBonuses[index].remainingAmount, $scope.grantedBonuses[index].currency, 2) +
                    ". Plesae note that this action is irreversible."
            });
        };
        $scope.moveToTop = function (index) {
            auth.moveToTop($scope.grantedBonuses[index].id).then(function () {
                var bonus = angular.copy($scope.grantedBonuses[index]);
                $scope.grantedBonuses.splice(index, 1);
                $scope.grantedBonuses.splice(0, bonus);
            });
        };
    }
})();