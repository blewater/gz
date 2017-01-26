var AsyncLoad = (function () {
    'use strict';

    var styles = [], scripts = [];
    var asyncLoad = {};

    function hidePreloader() {
        var preloader = document.getElementById("preloader");
        preloader.className = "die";
        setTimeout(function () {
            var body = document.getElementsByTagName("BODY")[0];
            body.removeChild(preloader);
        }, 200);
    }
    asyncLoad.AddStyles = function (src) {
        styles.push(src);
    };
    asyncLoad.LoadStyles = function () {
        loadStyles(styles);
    };
    function loadStyles(styles) {
        if (styles.length > 0) {
            var style = styles.splice(0, 1)[0];
            var cb = function () { loadStyles(styles); };
            loadStyle(style, cb);
        }
        else {
            hidePreloader();
            //removeUnnecessaryScripts();
        }
    }
    function removeUnnecessaryScripts() {
        var body = document.getElementsByTagName("BODY")[0];
        var allScripts = document.getElementsByTagName("SCRIPT");
        for (var i = 0; i < allScripts.length; i++) {
            var script = allScripts[i];
            if (script.getAttribute("data-remove"))
                body.removeChild(script);
        }
    }
    function loadStyle(src, callback) {
        var cb = function () {
            var l = document.createElement('link'); l.rel = 'stylesheet';
            l.href = src;
            var h = document.getElementsByTagName('head')[0]; h.parentNode.appendChild(l, h);
            if (typeof callback === 'function') {
                callback();
            }
        };
        var raf = requestAnimationFrame || mozRequestAnimationFrame ||
            webkitRequestAnimationFrame || msRequestAnimationFrame;
        if (raf) raf(cb);
        else window.addEventListener('load', cb);
    };


    asyncLoad.AddScript = function (src, callback) {
        scripts.push({ src: src, callback: callback });
    };
    asyncLoad.LoadScripts = function (srcs, callback) {
        if (Array.isArray(srcs) && srcs.length > 0) {
            var src = srcs.splice(0, 1)[0];
            var cb = srcs.length === 1 ? callback : function () { asyncLoad.LoadScripts(srcs, callback); };
            loadScript(src, cb);
        }
        else if (typeof srcs === 'string')
            loadScript(srcs, callback);
    }
    function loadScript(src, callback) {
        var s, r, t, a, d;
        r = false;
        s = document.createElement('script');
        s.type = 'text/javascript';
        s.src = src;
        //a = document.createAttribute("async");
        //s.setAttributeNode(a);
        d = document.createAttribute("defer");
        s.setAttributeNode(d);
        //if (defer) {
        //    s.setAttribute(document.createAttribute("async"));
        //    s.setAttribute(document.createAttribute("defer"));
        //}

        s.onload = s.onreadystatechange = function () {
            //console.log( this.readyState ); //uncomment this line to see which ready states are called.
            if (!r && (!this.readyState || this.readyState == 'complete')) {
                console.log(src);
                r = true;
                if (typeof callback === 'function')
                    callback();
            }
        };
        t = document.getElementsByTagName('script')[0];
        t.parentNode.appendChild(s);
    }
    return asyncLoad;
})(AsyncLoad);

