(function () {
    'use strict';

    APP.directive('gzThirdPartyIframe', ['helpers', 'constants', '$rootScope', directiveFactory]);

    function directiveFactory(helpers, constants, $rootScope) {
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
                    if (args.status === "success")
                        $scope.$parent.nsOk({ $pid: args.pid });
                    else if (args.status === "cancel")
                        $scope.$parent.nsCancel("The transaction is canceled.");
                    else
                        $scope.$parent.nsCancel("The transaction failed. (" + args.desc + ")");
                }

                $scope.$on(constants.events.DEPOSIT_STATUS_CHANGED, transactionStatusChanged);
                $scope.$on(constants.events.WITHDRAW_STATUS_CHANGED, transactionStatusChanged);
            }]
        };
    }
})();