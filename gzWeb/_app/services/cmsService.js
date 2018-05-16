(function () {
    'use strict';

    APP.factory('cms', [serviceFactory]);
    function serviceFactory() {
        var butterService = Butter('8ed2eeb05a038efe7ba52d2a9804d33bf7e2a613');
        return butterService;
    };
})();