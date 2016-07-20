(function () {
    'use strict';

    APP.factory('helpers', ['$window', '$timeout', '$compile', '$templateRequest', '$controller', '$rootScope', 'constants', 'localStorageService', 'screenSize', serviceFactory]);
    function serviceFactory($window, $timeout, $compile, $templateRequest, $controller, $rootScope, constants, localStorageService, screenSize) {
        var service = {
            reflection: {
                hasValue: hasValue,
                getProperty: getProperty,
                getPropertyOrSelf: getPropertyOrSelf
            },
            array: {
                chunk: chunk,
                any: any,
                all: all,
                aggregate: aggregate,
                shuffle: shuffle,
                swap: swap,
                applyWithDelay: applyWithDelay,
                rotate: rotate
            },
            ui: {
                getTemplate: getTemplate,
                compile: compile,
                openInNewTab: openInNewTab,
                isFocusable: isFocusable,
                isMobile: isMobile,
                watchScreenSize: watchScreenSize
            },
            utils: {
                guid: guid,
                random: random,
                closure: closure,
                prettyPrintEmail: prettyPrintEmail
            }
        };
        return service;

        // #region reflection
        function hasValue(variable) {
            return variable !== undefined && variable != null;
        }
        function getProperty(model, name) {
            return hasValue(model) && hasValue(name) ? model[name] : undefined;
        }
        function getPropertyOrSelf(model, name) {
            return hasValue(model) && hasValue(name) ? getProperty(model, name) : model;
        }
        // #endregion

        // #region array
        function chunk(array, n) {
            var i, j, newArray = [];
            for (i = 0, j = array.length; i < j; i += n)
                newArray.push(array.slice(i, i + n));
            return newArray;
        }
        function any(array, predicate) {
            var predicateSucceeded = false;
            if (array.length > 0 && angular.isFunction(predicate)) {
                for (var i = 0; i < array.length; i++) {
                    if (predicate(array[i])) {
                        predicateSucceeded = true;
                        break;
                    }
                }
            }
            return predicateSucceeded;
        }
        function all(array, predicate) {
            var predicateSucceeded = false;
            if (array.length > 0 && angular.isFunction(predicate)) {
                predicateSucceeded = true;
                for (var i = 0; i < array.length; i++) {
                    if (!predicate(array[i])) {
                        predicateSucceeded = false;
                        break;
                    }
                }
            }
            return predicateSucceeded;
        }
        function aggregate(array, seed, func) {
            if (array.length > 0 && angular.isFunction(func))
                for (var i = 0; i < array.length; i++)
                    seed = func(seed, array[i]);
            return seed;
        }
        function shuffle(array) {
            var n = array.length;
            while (n) {
                var i = Math.floor(Math.random() * n--);
                var t = array[n];
                array[n] = array[i];
                array[i] = t;
            }
            return array;
        }
        function swap(array, i, j) {
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
        function applyWithDelay(array, func, delay, callback) {
            if (array.length === 0) {
                if (angular.isFunction(callback))
                    callback();
            }
            else {
                func(array[0]);
                $timeout(function () {
                    array.shift();
                    applyWithDelay(array, func, delay, callback);
                }, delay);
            }
        }
        function rotate(array, count) {
            return array.push.apply(array, array.splice(0, count % array.length))
        }
        // #endregion

        // #region ui
        function getTemplate(templateUrl) {
            var version = localStorageService.get(constants.storageKeys.version);// || constants.version;
            //var debug = localStorageService.get(constants.storageKeys.debug) || constants.debugMode;
            var randomSuffix = localStorageService.get(constants.storageKeys.randomSuffix);
            if (!randomSuffix) {
                randomSuffix = Math.random();
                localStorageService.set(constants.storageKeys.randomSuffix, randomSuffix);
            }

            //var suffix = debug
            //    ? randomSuffix
            //    : 'v.' + version + '_' + randomSuffix;
            var suffix = 'v.' + version + '_' + randomSuffix;
            return templateUrl + '?' + suffix;
        }
        function compile(selector, templateUrl, controllerId, scope) {
            $templateRequest(getTemplate(templateUrl)).then(function (html) {
                var $content = angular.element(selector);
                $content.contents().remove();
                $content.html(html);
                if (angular.isDefined(controllerId)) {
                    if (angular.isUndefined(scope))
                        scope = $rootScope.$new();
                    var ctrl = $controller(controllerId, { $scope: scope });
                    $content.children().data('$ngControllerController', ctrl);
                }
                $compile($content.contents())(scope);
            });
        }

        function openInNewTab(url) {
            $window.open(url, '_blank');
        }
        function isFocusable(element) {
            if (!element.is(":hidden") && !element.is(":disabled"))
                return false;

            var tabIndex = +element.attr("tabindex");
            tabIndex = isNaN(tabIndex) ? -1 : tabIndex;
            return element.is(":input, a[href], area[href], iframe") || tabIndex > -1;
        }
        function isMobile() {
            function mobileCheck() {
                var check = false;
                (function (a) {
                    if (/(android|ipad|playbook|silk|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) {
                        check = true;
                    }
                })(navigator.userAgent || navigator.vendor || window.opera);
                return check;
            }
            return mobileCheck();
        }
        function watchScreenSize(scope) {
            scope.xs = screenSize.on('xs', function (match) { scope.xs = match; });
            scope.sm = screenSize.on('sm', function (match) { scope.sm = match; });
            scope.md = screenSize.on('md', function (match) { scope.md = match; });
            scope.lg = screenSize.on('lg', function (match) { scope.lg = match; });
            scope.size = screenSize.get();
            screenSize.on('xs,sm,md,lg', function () {
                scope.size = screenSize.get();
            });
        }
        // #endregion

        // #region utils
        function guid() {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
            }
            return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
        }
        function random(min, max) {
            Math.floor((Math.random() * max) + min);
        }
        function closure(func, ctx) {
            return function (data) {
                return func(ctx, data);
            };
        }
        function prettyPrintEmail(email) {
            var splitted = email.split("@");
            return splitted[0].concat(" -at- ").concat(splitted[1].replace(".", " -dot- "));
        }
        // #endregion
    };
})();