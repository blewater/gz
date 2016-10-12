﻿(function () {
    'use strict';

    APP.directive('gzThirdPartyIframe', ['helpers', 'constants', '$rootScope', 'message', directiveFactory]);

    function directiveFactory(helpers, constants, $rootScope, message) {
        return {
            restrict: 'E',
            scope: {
                gzRedirectionForm: '='
            },
            replace: true,
            templateUrl: function () {
                return helpers.ui.getTemplate('_app/directives/gzThirdPartyIframe.html');
            },
            controller: ['$scope', '$element', '$attrs', function ($scope, $element, $attrs) {
                $rootScope.loading = true;
                var unregisterRedirectionFormWatch = $scope.$watch("gzRedirectionForm", function (newValue, oldValue) {
                    if (newValue && newValue.length > 0) {
                        var $wrapper = $('#redirection-form-wrapper');
                        $wrapper.html($scope.gzRedirectionForm);
                        $('form', $wrapper).attr('target', 'third-party-iframe').attr('method', 'POST');
                        $('form', $wrapper).eq(0).submit();
                        unregisterRedirectionFormWatch();
                        $rootScope.loading = false;
                    }
                });

                function transactionStatusChanged(event, args) {
                    if (args.status === "success") {
                        $rootScope.$broadcast(constants.events.REQUEST_ACCOUNT_BALANCE);
                        $scope.$parent.nsOk({ $pid: args.pid });
                    }
                    else if (args.status === "cancel")
                        $scope.$parent.nsCancel("Your bank has declined your transaction. Please contact your issuing bank to resolve this issue or try with another payment method or card.");
                    else
                        $scope.$parent.nsCancel("The transaction failed. (" + args.desc + ")");
                }

                $scope.$on(constants.events.DEPOSIT_STATUS_CHANGED, transactionStatusChanged);
                $scope.$on(constants.events.WITHDRAW_STATUS_CHANGED, transactionStatusChanged);

                $scope.close = function () {
                    message.confirm("Are you sure you want to cancel?", function () {
                        $scope.$parent.nsCancel("The transaction was canceled.");
                    });
                };
            }]
        };
    }
})();