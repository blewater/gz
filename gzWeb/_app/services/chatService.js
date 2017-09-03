(function () {
    'use strict';

    APP.factory('chat', ['$rootScope', 'constants', serviceFactory]);
    function serviceFactory($rootScope, constants) {
        var _duration = 400;

        var service = {
            show: show,
            hide: hide,
            isMin: isMin,
            isMax: isMax,
            min: min,
            max: max
        };
        return service;

        function show() {
            if ($('.zopim').length > 0 && $('.zopim:nth-of-type(1)').is(':hidden') && $('.zopim:nth-of-type(2)').is(':hidden')) {
                $('.zopim:nth-of-type(1)').fadeIn(_duration);
            }
            else
                init();
        }
        function hide() {
            $('.zopim').fadeOut(_duration);
        }

        function isMin() {
            return $('.zopim').length > 0 && $('.zopim:nth-of-type(1)').is(':visible') && $('.zopim:nth-of-type(2)').is(':hidden');
        }
        function isMax() {
            return $('.zopim').length > 0 && $('.zopim:nth-of-type(1)').is(':hidden') && $('.zopim:nth-of-type(2)').is(':visible');
        }
        function min() {
            toggle(true);
        }
        function max() {
            toggle(false);
        }
        function toggle(min) {
            if ($('.zopim').length > 0) {
                if (min) {
                    $('.zopim:nth-of-type(1)').show();
                    $('.zopim:nth-of-type(2)').hide();
                }
                else {
                    $('.zopim:nth-of-type(1)').hide();
                    $('.zopim:nth-of-type(2)').show();
                }
            }
        }

        function _onLoad() {
            //show();
            $rootScope.$broadcast(constants.events.CHAT_LOADED);
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
                z.s.async = true;
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