(function(ng) {
    'use strict';

    ng.module('logToServer', [])
        .service('$log', function() {
            this.log = function(msg) {
                JL('gzWebClient').info(msg);
            }
            this.trace = function (msg) {
                JL('gzWebClient').trace(msg);
            }
            this.debug = function(msg) {
                JL('gzWebClient').debug(msg);
            }

            this.info = function(msg) {
                JL('gzWebClient').info(msg);
            }

            this.warn = function(msg) {
                JL('gzWebClient').warn(msg);
            }

            this.error = function(msg) {
                JL('gzWebClient').error(msg);
            }
        })
        .factory('$exceptionHandler', function() {
            return function(exception, cause) {
                JL('Angular').fatalException(cause, exception);
                throw exception;
            };
        })
        .factory('logToServerInterceptor', [
            '$q', function($q) {
                var myInterceptor = {
                    'request': function(config) {
                        config.msBeforeAjaxCall = new Date().getTime();
                        return config;
                    },
                    'response': function(response) {
                        if (response.config.warningAfter) {
                            var msAfterAjaxCall = new Date().getTime();
                            var timeTakenInMs = msAfterAjaxCall - response.config.msBeforeAjaxCall;
                            if (timeTakenInMs > response.config.warningAfter) {
                                JL('gzWebClient.Ajax').warn({ "timeTakenInMs": timeTakenInMs, config: response.config, data: response.data });
                            }
                        }

                        return response;
                    },
                    'responseError': function(rejection) {
                        var errorMessage = "timeout";
                        if (rejection && rejection.status && rejection.data) {
                            errorMessage = rejection.data.ExceptionMessage;
                        }

                        JL('gzWebClient.Ajax').fatalException({ errorMessage: errorMessage, status: rejection.status, config: rejection.config }, rejection.data);
                        return $q.reject(rejection);
                    }
                };

                return myInterceptor;
            }
        ]);
}(angular));