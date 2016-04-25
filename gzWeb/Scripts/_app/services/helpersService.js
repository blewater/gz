(function () {
    'use strict';

    APP.factory('helpers', ['$window', '$timeout', '$compile', '$templateRequest', 'constants', 'localStorageService', serviceFactory]);
    function serviceFactory($window, $timeout, $compile, $templateRequest, constants, localStorageService) {
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
                shuffle: shuffle,
                swap: swap,
                applyWithDelay: applyWithDelay
            },
            ui: {
                getTemplate: getTemplate,
                compileTemplate: compileTemplate,
                openInNewTab: openInNewTab,
                isFocusable: isFocusable
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
            for (i = 0, j = array.length; i < j; i += n) {
                newArray = array.slice(i, i + n);
            }
            return newArray;
        }
        function any(array, predicate) {
            var predicateSucceeded = false;
            if (array.length > 0 && isFunction(predicate)) {
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
            if (array.length > 0 && isFunction(predicate)) {
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
            if (array.length === 0)
                if (angular.isFunction(callback))
                    callback();
            else {
                func(array[0]);
                $timeout(function () {
                    array.shift();
                    applyWithDelay(array, func, delay, callback);
                }, delay);
            }
        }
        // #endregion

        // #region ui
        function getTemplate(templateUrl) {
            var randomSuffix = localStorageService.get(constants.storageKeys.randomSuffix);
            if (!randomSuffix) {
                randomSuffix = Math.random();
                localStorageService.set(constants.storageKeys.randomSuffix, randomSuffix);
            }

            var suffix = constants.debugMode ? randomSuffix : 'v.' + constants.version + '_' + randomSuffix;
            return templateUrl + '?' + suffix;
        }
        function compileTemplate(templateUrl, scope, callback) {
            $templateRequest(getTemplate(templateUrl)).then(function (tpl) {
                var container = angular.element('<div></div>');
                var compiled = $compile(tpl)(scope);
                container.append(compiled);
                $timeout(function () { callback(container.html()); }, 0);
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