var AsyncLoad = (function () {
    'use strict';

    var styles = [], scripts = [], total = 0;
    var asyncLoad = {};

    // #region Styles
    function loadStyle(src, callback) {
        var cb = function () {
            var l = document.createElement('link');
            l.rel = 'stylesheet';
            l.href = src;
            var h = document.getElementsByTagName('BODY')[0];
            h.appendChild(l, h);
            setPercent();
            if (typeof callback === 'function')
                callback();
        };
        var raf = requestAnimationFrame || mozRequestAnimationFrame || webkitRequestAnimationFrame || msRequestAnimationFrame;
        if (raf)
            raf(cb);
        else
            window.addEventListener('load', cb);
    };
    function loadStyles() {
        if (styles.length > 0) {
            var style = styles.splice(0, 1)[0];
            loadStyle(style, function () {
                loadStyles(styles);
            });
        }
    };
    asyncLoad.AddStyles = function (src) {
        styles.push(src);
    };
    // #endregion

    // #region Scripts
    function loadScript(src, callback) {
        var s, r, t, a, d;
        r = false;
        s = document.createElement('script');
        s.type = 'text/javascript';
        s.src = src;
        //a = document.createAttribute("async");
        //s.setAttributeNode(a);
        //d = document.createAttribute("defer");
        //s.setAttributeNode(d);
        //if (defer) {
        //    s.setAttribute(document.createAttribute("async"));
        //    s.setAttribute(document.createAttribute("defer"));
        //}

        s.onload = s.onreadystatechange = function () {
            //console.log( this.readyState ); //uncomment this line to see which ready states are called.
            if (!r && (!this.readyState || this.readyState == 'complete')) {
                r = true;
                setPercent();
                if (typeof callback === 'function')
                    callback();
            }
        };
        var b = document.getElementsByTagName('BODY')[0];
        b.appendChild(s);
        //t = document.getElementsByTagName('script')[0];
        //t.parentNode.appendChild(s);
    };
    function loadScripts() {
        if (scripts.length > 0) {
            var script = scripts.splice(0, 1)[0];
            loadScript(script.src, function () {
                if (typeof script.callback === 'function')
                    script.callback();
                loadScripts(scripts);
            });
        }
    };
    asyncLoad.AddScripts = function (src, callback) {
        scripts.push({ src: src, callback: callback });
    };
    // #endregion

    // #region Common
    function getCount() {
        return styles.length + scripts.length;
    };
    function setPercent() {
        var count = getCount();
        var percent = Math.floor(((total - count) / total) * 100);
        var element = document.getElementById("loading-percentage");
        element.innerHTML = '';
        element.appendChild(document.createTextNode(percent + " %"));
    };
    function removeUnnecessaryScripts(attr) {
        var allScripts = document.getElementsByTagName("SCRIPT");
        var scriptsToRemove = [], i;
        for (i = 0; i < allScripts.length; i++) {
            var script = allScripts[i];
            if (script.getAttribute(attr))
                scriptsToRemove.push(script);
        }
        for (i = 0; i < scriptsToRemove.length; i++) {
            var script = scriptsToRemove[i];
            script.parentElement.removeChild(script);
        }
    };
    asyncLoad.Load = function () {
        total = getCount();
        loadStyles();
        loadScripts();
        removeUnnecessaryScripts("data-remove");
    };
    // #endregion

    return asyncLoad;
})(AsyncLoad);

