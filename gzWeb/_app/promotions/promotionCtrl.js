(function () {
    'use strict';
    var ctrlId = 'promotionCtrl';
    APP.controller(ctrlId, ['$scope', 'api', '$routeParams', '$location', '$controller', 'message', 'accountManagement', ctrlFactory]);
    function ctrlFactory($scope, api, $routeParams, $location, $controller, message, accountManagement) {
        $controller('authCtrl', { $scope: $scope });

        function loadPage() {
            var code = $routeParams.code;
            api.call(function () {
                return api.getPage(code);
            }, function (response) {
                $scope.html = response.Result;
            });
        }

        function loadAuthData() {
            $scope.isLoggedIn = $scope._authData.isGamer;
        }

        $scope._init(function () {
            loadAuthData();
            loadPage();
        });

        $scope.go = function (actionUrl) {
            switch (actionUrl) {
                case 'deposit':
                    signUp(function () {
                        accountManagement.open(accountManagement.states.depositPaymentMethods);
                    });
                    break;
                case 'bonus':
                    signUp(function () {
                        accountManagement.open(accountManagement.states.bonuses);
                    });
                    break;
                default:
                    signUp(function () {
                        $location.url(actionUrl);
                    });
                    break;
            }
        };

        function signUp(callback) {
            var signUpCallback = angular.isDefined(callback) ? callback : angular.noop;
            if ($scope.isLoggedIn)
                signUpCallback();
            else {
                var signUpPromise = message.open({
                    nsType: 'modal',
                    nsSize: '600px',
                    nsTemplate: '_app/account/registerAccount.html',
                    nsCtrl: 'registerAccountCtrl',
                    nsStatic: true
                });
                signUpPromise.then(signUpCallback);
            }
        }
    }
})();
