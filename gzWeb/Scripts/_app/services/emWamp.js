(function () {
    'use strict';

    APP.factory("emWamp", ['$wamp', function($wamp) {

        $wamp.open();

            var service = {
                call: function(uri, parameters) {
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
                }
            };

            return service;
        }
    ]);

})();