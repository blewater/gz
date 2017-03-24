(function() {
    'use strict';

    APP.constant("emConCfg", {
        webSocketApiUrl: 'wss://greenzorro-api.everymatrix.com/v2',//'wss://api3.everymatrix.com/v2',
        fallbackApiUrl: 'https://greenzorro-api.everymatrix.com/longpoll',//'https://comet3.everymatrix.com',
        domainPrefix: 'http://www.greenzorro.com'
    });
})();