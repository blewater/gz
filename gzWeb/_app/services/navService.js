﻿(function () {
    'use strict';

    APP.factory('nav', [serviceFactory]);
    function serviceFactory() {
        var requestUrl = "";
        var previousRequestUrl = "";

        var service = {
            setRequestUrl: setRequestUrl,
            getRequestUrl: getRequestUrl,
            getPreviousRequestUrl: getPreviousRequestUrl,
            clearRequestUrls: clearRequestUrls
        };
        return service;

        function setRequestUrl(url) {
            if (url) {
                if (requestUrl)
                    previousRequestUrl = requestUrl;
                requestUrl = url;
            }
        };
        function getRequestUrl() {
            return requestUrl;
        };
        function getPreviousRequestUrl() {
            return previousRequestUrl;
        };
        function clearRequestUrls() {
            requestUrl = "";
            previousRequestUrl = "";
        };
    };
})();