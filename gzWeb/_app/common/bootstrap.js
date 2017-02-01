(function () {
    'use strict';

    function hidePreloader() {
        var preloader = document.getElementById("preloader");
        preloader.className = "die";
        setTimeout(function () {
            var body = document.getElementsByTagName("BODY")[0];
            body.removeChild(preloader);
        }, 1000);
    };

    function showContent() {
        var content = document.getElementById("body-content");
        content.className = "ok";

        var loading = document.getElementById("loading");
        loading.className = "ok";

        var headerNav = document.getElementById("header-nav");
        headerNav.className += " navbar-fixed-top";
    };

    function loadWebFonts() {
        WebFont.load({
            google: {
                families: ['Fira Sans']
            }
        });
    }

    angular.element(document).ready(function () {
        angular.bootstrap(document, [APP.id]);
        hidePreloader();
        showContent();
    });
})();