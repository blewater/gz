(function () {
    'use strict';

    APP.factory('route', ['$location', '$window', 'constants', '$filter', serviceFactory]);
    function serviceFactory($location, $window, constants, $filter) {
        var service = {
            getPaths: getPaths,
            getTemplates: getTemplates,
            getGroup: getGroup,
            getRoute: getRoute,
            getPath: getPath,
            getTemplate: getTemplate,
            isCurrentPath: isCurrentPath,
            isSubPathOf: isSubPathOf,
        };
        return service;

        function getPaths() {
            return $filter('map')(routes, 'path');
        }
        function getTemplates() {
            return $filter('map')(routes, 'tpl');
        }
        function getGroup(groupKey) {
            return $filter('where')(constants.routes, { 'group': groupKey });
        }
        function getRoute(key) {
            return $filter('where')(constants.routes, { 'key': key });
        }
        function getPath(key) {
            return getRoute(key).path;
        }
        function getTemplate(key) {
            return getRoute(key).tpl;
        }
        function isCurrentPath(path) {
            return $location.path() === path;
        }
        function isSubPathOf(path) {
            var startIndex = constants.html5Mode ? 1 : 2;
            var current = $location.path();
            return current.substr(startIndex).indexOf(path) == 0;
        }
    };
})();