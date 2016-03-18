// This file wraps the autobanhn.js


(function () {
    // Represents if fallback is required
    var enableFallback = false;

    // First, it checks the WebSocket support in web browser. If not,  fallback will be enabled.
    if (typeof (WebSocket) != "function")
        enableFallback = true;

    if( !enableFallback ){
        // Second, websocket connection could be blocked by proxy(http://www.infoq.com/articles/Web-Sockets-Proxy-Servers) or other intermediate nodes
        // If previous websocket connection attempt failed, should enable fallback now
        // The previous status is stored in localStorage, according to http://caniuse.com/#search=localstorage & http://caniuse.com/#search=websocket
        // If websocket is supported, localStorage must be supported.
        var lastFailureTime = localStorage.getItem('lastFailureTime');
        if (!isNaN(lastFailureTime)) {
            var offset = (new Date()).getTime() - lastFailureTime;
            // If the last failure happened within recent 1 hour, enable fallback
            enableFallback = offset < 3600000 && offset >= 0;
        }
    }
	
	if (getQueryString('force_ws') != '')
		enableFallback = false;
	if (getQueryString('force_fb') != '')
		enableFallback = true;
	
    // include dummy-websocket.js to enable fallback
    if( enableFallback )
        document.writeln('<script src="dummy-websocket.js"></script>');

})();


   


SessionStatus = {
    NOT_CONNECTED: "not connected",
    CONNECTING: "connecting",
    CONNECTED: "connected",
    RESUMING_LOGIN: "resuming login",
    INIT_COMPLETED: "initialization completed",
    LOGGED_IN: "login",
    LOGGED_OUT: "logout"
};

SessionMgr = {
    conn: null, // the connection
    s: null, // the session
    _status: SessionStatus.NOT_CONNECTED,

    errorHandlers: new Observer(),
    statusChangeHandlers: new Observer(),


    notifyStatusChange: function (status, code, reason) {
        if (status != SessionStatus.RESUMING_LOGIN &&
            status != SessionStatus.INIT_COMPLETED) {
            SessionMgr._status = status;
            if (status == SessionStatus.LOGGED_OUT)
                SessionMgr._status = SessionStatus.CONNECTED;
        }

        SessionMgr.statusChangeHandlers.publish({
            status: status,
            code: code,
            reason: reason
        });
    },

    isLoggedIn: function () {
        return SessionMgr._status == SessionStatus.LOGGED_IN;
    },




    // establish the connection
    init: function () {
        if (typeof (WEBSOCKET_API_URL) != typeof (''))
            throw '[WEBSOCKET_API_URL] is not defined!';

        if (typeof (DOMAIN_PREFIX) != typeof (''))
            throw '[DOMAIN_PREFIX] is not defined!';

        var COOKIE_NAME = '$sessionID$';


        SessionMgr.notifyStatusChange(SessionStatus.CONNECTING);
        SessionMgr.conn = new autobahn.Connection({ url: WEBSOCKET_API_URL, realm: DOMAIN_PREFIX });

        // If websocket connection is being established, detect its failure
        var timer = null;
        if (WebSocket.isFallback != true) {
            // It is not safe to detect the websocket connection failure via callback 
            // as some intermediate node may prevent the websocket handshake and no callback will be fired.
            // here take use a timer and if the connection can not be established within 5 seconds, then re-try in fallback
            timer = setTimeout(function () {
                localStorage.setItem('lastFailureTime', (new Date()).getTime()); // save the last fail time
                self.location = self.location; // refresh current page and it will re-try with fallback
            }, 5000);
        }

        SessionMgr.conn.onopen = function (session) {
            if (timer != null) {
                clearTimeout(timer);
                timer = null;
            }


            SessionMgr.s = session;
            SessionMgr.notifyStatusChange(SessionStatus.CONNECTED);

            SessionMgr.subscribe("/sessionStateChange", function (data) {
                if (data.code == 0) { // 0 means this user is logged-in
                    CookieHelper.appendToArray(COOKIE_NAME, session.id); // save
                    SessionMgr.notifyStatusChange(SessionStatus.LOGGED_IN);
                } else {
                    CookieHelper.removeFromArray(COOKIE_NAME, session.id);
                    SessionMgr.notifyStatusChange(SessionStatus.LOGGED_OUT, data.code, data.desc);
                }
            }).then(
                // after the event is subscribed, set the group id
                function (subscription) {
                    
                    // join group
                    var parameter = { groupID: CookieHelper.get('$client_id$') };
                    SessionMgr.call("/user#setClientIdentity", parameter).then(
                        function (result) {
                            CookieHelper.set('$client_id$', result.groupID);
                            SessionMgr.notifyStatusChange(SessionStatus.INIT_COMPLETED);
                        }
                        , function (err) {
                            if (typeof (console) != null && typeof (console.log) == 'function')
                                console.log(err.desc);
                            SessionMgr.notifyStatusChange(SessionStatus.INIT_COMPLETED);
                        }
                    );
                },
                function (errorCode) {
                    
                    SessionMgr.notifyStatusChange(SessionStatus.NOT_CONNECTED);
                }
            );

        };

        SessionMgr.conn.onclose = function (status, data) {
            SessionMgr.notifyStatusChange(SessionStatus.NOT_CONNECTED);
        };

        SessionMgr.conn.open();
    },

    // Wrap the native "call" method from autobahn.js.
    // 1. Add a common parameter "language" which is loaded from cookie
    // 2. convert the input / output type for backward compatibility of WAMP v1.0
    call: function () {

        if (SessionMgr.s != null) {
            if( arguments.length == 0 )
                throw new Error("Not enough parameter");
            var procURI = arguments[0];
            var parameters = {};
            if( arguments.length > 1 )
                parameters = arguments[1];

            var lang = CookieHelper.get('language');
            if (lang != null && lang.length > 0) {
                parameters.culture = lang;
            }

            var callReturn = SessionMgr.s.call.apply(SessionMgr.s, [procURI, [], parameters]);

            // hook
            var orginalFunc = callReturn.then;
            callReturn.then = function (successCallback, failureCallback) {

                function _success(d) {
                    if (typeof (successCallback) === 'function')
                        successCallback(d && d.kwargs);
                }

                function _error(e) {
                    if (typeof (failureCallback) === 'function')
                        failureCallback(e.kwargs);
                }
                return orginalFunc.call(callReturn, _success, _error);
            };

            return callReturn;
        }
    },

    // Wrap the "subscribe" method from autobahn.js.
    // Convert the callback parameter for backward compatibility of WAMP v1.0
    subscribe: function (topicUri, callback) {
        return SessionMgr.s.subscribe(topicUri, function (args, kwargs, details) {
            if (typeof (callback) === 'function')
                callback(kwargs);
        });
    },

    // Wrap the "register" method from autobahn.js.
    register: function (procUri, callback) {
        return SessionMgr.s.register(procUri, function (args, kwargs, details) {
            if (typeof (callback) === 'function')
                callback(procUri, kwargs);
        });
    },

    // Wrap the "register" method from autobahn.js.
    unregister: function (procUri, callback) {
        return SessionMgr.s.unregister(procUri, function () {
        if (typeof (callback) === 'function')
            callback(procUri);
    });
}
};




