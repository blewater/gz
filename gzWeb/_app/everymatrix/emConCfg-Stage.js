(function() {
    'use strict';

    APP.constant("emConCfg", {
        webSocketApiUrl: 'wss://api-stage.everymatrix.com/v2',//'wss://webapi-stage.everymatrix.com/v2',
        fallbackApiUrl: 'https://api-stage.everymatrix.com/longpoll',//'https://fb-webapi-stage.everymatrix.com',
        domainPrefix: 'http://www.greenzorro.com'
    });
})();