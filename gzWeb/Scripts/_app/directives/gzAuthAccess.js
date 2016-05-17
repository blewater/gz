(function () {
    'use strict';

    APP.directive('gzAuthorize', ['auth', 'constants', authAccess])
       .directive('gzNotAuthorize', ['auth', 'constants', authNotAccess]);

    function authAccess(auth, constants) {
        return access(auth, constants, false);
    }

    function authNotAccess(auth, constants) {
        return access(auth, constants, true);
    }

    function access(auth, constants, not) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var determineVisibility = function () {
                    var roles = not ? attrs.gzNotAuthorize  : attrs.gzAuthorize;
                    var match = attrs.gzMatch || 'any';
                    var isAuthorized = auth.authorize(roles, match);
                    var mustHide = (isAuthorized && not) || (!isAuthorized && !not);
                    if (mustHide)
                        element.addClass('hidden');
                    else
                        element.removeClass('hidden');
                    if (attrs.gzChange)
                        scope.$eval(attrs.gzChange);
                };
                determineVisibility();
                scope.$on(constants.events.AUTH_CHANGED, determineVisibility);
            }
        };
    }
})();