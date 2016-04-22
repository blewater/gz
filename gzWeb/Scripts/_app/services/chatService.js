(function () {
    'use strict';

    APP.factory('chat', [serviceFactory]);
    function serviceFactory() {
        var _duration = 400;

        init();

        var service = {
            show: show,
            hide: hide
        };
        return service;

        function show() {
            $('.zopim:first-of-type').fadeIn(_duration);
        }
        function hide() {
            $('.zopim').fadeOut(_duration);
        }

        function _onLoad() {
            
        }

        function init() {
            window.$zopim || (function (d, s) {
                var $zopim = function(c) { z._.push(c); };
                var z = $zopim;
                z.s = d.createElement(s);
                var e = d.getElementsByTagName(s)[0];
                z.set = function (o) { z.set._.push(o); };
                z._ = [];
                z.set._ = [];
                z.s.async = !0;
                z.s.setAttribute("charset", "utf-8");
                z.s.src = "//v2.zopim.com/?3ql1n78VIwjWnWgVfrbuXG2sCMcCHPuM";
                z.s.addEventListener('load', _onLoad);
                z.t = +new Date;
                z.s.type = "text/javascript";
                e.parentNode.insertBefore(z.s, e);

                //var z = $zopim = function (c) { z._.push(c) }, $ = z.s =
                //d.createElement(s), e = d.getElementsByTagName(s)[0]; z.set = function (o) {
                //    z.set.
                //    _.push(o)
                //}; z._ = []; z.set._ = []; $.async = !0; $.setAttribute("charset", "utf-8");
                //$.src = "//v2.zopim.com/?3ql1n78VIwjWnWgVfrbuXG2sCMcCHPuM"; z.t = +new Date; $.
                //type = "text/javascript"; e.parentNode.insertBefore($, e)
            })(document, "script");
        }
    };
})();