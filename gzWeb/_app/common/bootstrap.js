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

    angular.element(document).ready(function () {
        angular.bootstrap(document, [APP.id]);
        hidePreloader();
    });
})();