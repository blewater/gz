(function () {
    'use strict';

    APP.factory('iovation', ['$window', '$timeout', serviceFactory]);
    function serviceFactory($window, $timeout) {
        $window.io_install_stm = false;
        $window.io_exclude_stm = 12;
        $window.io_install_flash = false;
        $window.io_enable_rip = true;
        $window.io_bb_callback = blackboxCallback;

        var delay = 5000;
        var blackbox = "";
        var sent = false;
        function setBlackbox(bb) {
            if (sent)
                return;
            sent = true;
            blackbox = bb;
        }

        $timeout(function () {
            try {
                setBlackbox(ioGetBlackbox().blackbox);
                return;
            }
            catch (e) {
                console.log(e);
            }
            setBlackbox("");
        }, delay);

        function blackboxCallback(bb, isComplete) {
            if (isComplete)
                setBlackbox(bb);
        };

        function getBlackbox() {
            return blackbox;
        }

        var service = {
            getBlackbox: getBlackbox
        };

        return service;
    };
})();