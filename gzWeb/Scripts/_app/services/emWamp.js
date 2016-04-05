(function() {
    'use strict';

    APP.factory("emWamp", ['$wamp', emWampFunction]);
    function emWampFunction($wamp) {

        var _logError = function(error) {
            console.log(error);
        };

        var _call = function(uri, parameters) {
            var callReturn = $wamp.call(uri, [], parameters);

            var originalFunc = callReturn.then;
            callReturn.then = function(successCallback, failureCallback) {
                function success(d) {
                    if (typeof (successCallback) === 'function')
                        successCallback(d && d.kwargs);
                }

                function error(e) {
                    if (typeof (failureCallback) === 'function')
                        failureCallback(e.kwargs);
                }

                return originalFunc.call(callReturn, success, error);
            };

            return callReturn;
        };

        var service = {

            call: _call,

            login: function() {
                
            },

            logout: function() {
                _call('/user#logout').then(function(result) {}, _logError);
            },
        };

        $wamp.open();

        return service;
    };

})();