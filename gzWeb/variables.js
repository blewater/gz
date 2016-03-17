// This file declares the connection variables 
// You can replace them with hard-code strings for your site

var WEBSOCKET_API_URL = 'wss://webapi-stage.everymatrix.com/v2';
var FALLBACK_API_URL = 'https://fb-webapi-stage.everymatrix.com';
var DOMAIN_PREFIX = 'http://www.greenzorro.com';

if (WEBSOCKET_API_URL == null || WEBSOCKET_API_URL.length == 0)
    WEBSOCKET_API_URL = getQueryString('WEBSOCKET_API_URL');
if (FALLBACK_API_URL == null || FALLBACK_API_URL.length == 0)
    FALLBACK_API_URL = getQueryString('FALLBACK_API_URL');
if (DOMAIN_PREFIX == null || DOMAIN_PREFIX.length == 0)
    DOMAIN_PREFIX = getQueryString('DOMAIN_PREFIX');



if (WEBSOCKET_API_URL == null || WEBSOCKET_API_URL.length == 0)
    WEBSOCKET_API_URL = CookieHelper.get('_WEBSOCKET_API_URL');
else
    CookieHelper.set('_WEBSOCKET_API_URL', WEBSOCKET_API_URL);

if (DOMAIN_PREFIX == null || DOMAIN_PREFIX.length == 0)
    DOMAIN_PREFIX = CookieHelper.get('_DOMAIN_PREFIX');
else
    CookieHelper.set('_DOMAIN_PREFIX', DOMAIN_PREFIX);

if (FALLBACK_API_URL == null || FALLBACK_API_URL.length == 0)
    FALLBACK_API_URL = CookieHelper.get('_FALLBACK_API_URL');
else
    CookieHelper.set('_FALLBACK_API_URL', FALLBACK_API_URL);

if (WEBSOCKET_API_URL == null || WEBSOCKET_API_URL == '')
    throw new Error("[WEBSOCKET_API_URL] is null.");

if (DOMAIN_PREFIX == null || DOMAIN_PREFIX == '')
    throw new Error("[DOMAIN_PREFIX] is null.");

if (FALLBACK_API_URL == null || FALLBACK_API_URL == '')
    throw new Error("[FALLBACK_API_URL] is null.");


if (!/(\/v2)$/.test(WEBSOCKET_API_URL))
    WEBSOCKET_API_URL += '/v2';