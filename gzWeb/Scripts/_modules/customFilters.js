(function() {
    'use strict';
    angular.module('customFilters', [])
        .filter('partition', ['$cacheFactory', partition])
        .filter('rangeFromToStep', rangeFromToStep)
        .filter('rangeFromTo', rangeFromTo)
        .filter('rangeTo', rangeTo)
        .filter('rangeToStep', rangeToStep)
        .filter('regex', regex)
        .filter('pad', pad)
        .filter('type', ['$filter', dataType])
        .filter('filterChild', filterChild)
    ;

    // #region partition
    function partition($cacheFactory) {
        var arrayCache = $cacheFactory('partition');
        var filter = function (arr, size) {
            if (!arr) { return; }
            var newArr = [];
            for (var i = 0; i < arr.length; i += size) {
                newArr.push(arr.slice(i, i + size));
            }
            var cachedParts;
            var arrString = JSON.stringify(arr);
            cachedParts = arrayCache.get(arrString + size);
            if (JSON.stringify(cachedParts) === JSON.stringify(newArr)) {
                return cachedParts;
            }
            arrayCache.put(arrString + size, newArr);
            return newArr;
        };
        return filter;
    }
    // #endregion

    // #region range
    var range = function (input, from, to, step) {
        from = parseInt(from);
        to = parseInt(to);
        step = parseInt(step);
        while (from + step <= to)
            input[input.length] = from += step;
        return input;
    };
    function rangeFromToStep() {
        return range;
    };
    function rangeFromTo() {
        return function (input, from, to) {
            return range(input, from, to, 1);
        };
    };
    function rangeTo() {
        return function (input, to) {
            return range(input, 0, to, 1);
        };
    };
    function rangeToStep() {
        return function (input, to, step) {
            return range(input, 0, to, step);
        };
    };
    // #endregion

    // #region regex
    function regex() {
        return function (input, field, pattern) {
            var patt = new RegExp(pattern);
            var out = [];
            for (var i = 0; i < input.length; i++) {
                if (patt.test(input[i][field]))
                    out.push(input[i]);
            }
            return out;
        };
    }
    // #endregion

    // #region pad
    function pad() {
        return function (num, places, padWith) {
            var minus = "-";
            if (!num)
                num = "";
            var text = num.toString();
            if (text.length >= places)
                return text;

            var isNegative = text.substring(0, 1) === minus;
            var suffix = isNegative ? minus : "";
            var numText = isNegative ? text.substring(1) : text;

            padWith = (padWith || 0).toString();
            var times = places - text.length;
            var padding = padWith.repeat(times);
            return suffix + padding + numText;
        };
    };
    // #endregion

    // #region filterChild
    function filterChild() {
        return function (items, field, term) {
            var result = {};
            angular.forEach(items, function (item) {
                if (item[field].contains(term)) {
                    result.push(item);
                }
            });
            return result;
        };
    };    // #endregion

    // #region type
    function dataType($filter) {
        return function (input, type) {
            if (input == Infinity)
                return null;

            var digits = 2;

            if (type.indexOf(':') > -1) {
                digits = parseInt(type.split(':')[1].trim());
                type = type.split(':')[0].trim();
            }

            switch (type) {
                case 'text':
                    return input;
                case 'number':
                    return $filter(type)(input, digits);
                case 'percentage':
                    return $filter('number')(input, digits) + '%';
                case 'dateTime':
                    return $filter('date')(input, 'dd-MMM-yyyy HH:mm:ss');
                default:
                    return input;
            }
        }
    };
    // #endregion
})();


