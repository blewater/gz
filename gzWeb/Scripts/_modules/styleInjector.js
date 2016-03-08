(function () {
    'use strict';

    angular.module('styleInjector', [])
        .factory('styleInjector', function() {
            var style = angular.element('<style>'), initializedDirectives = {};

            angular.element('head').append(style);

            return {
                insertCSS: insertCSS,
                importCSSLink: importCSSLink,
                initDirective: initDirective,

            };

            function insertCSS(css) {
                style.text(style.text() + css + '\n');
            }

            function importCSSLink(url) {
                insertCSS('@import url(' + JSON.stringify(url) + ');');
            }

            function initDirective(desc) {
                // A helper for directives to add their styles once.
                if (!initializedDirectives[desc.name]) {
                    if (desc.css)
                        insertCSS(desc.css);
                    if (desc.cssUrl)
                        importCSSLink(desc.cssUrl);
                    initializedDirectives[desc.name] = true;
                }
            }
        });
})();


