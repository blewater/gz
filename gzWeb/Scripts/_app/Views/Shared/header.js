(function() {
    "use strict";

    $(function () {
        var $collapse = $('#header-collapse-section');
        $('.nav-btn-login').click(function () {
            $collapse.removeClass('guest-mode investments-mode');
            $collapse.addClass('casino-mode');
        });
        $('.nav-btn-casino').click(function () {
            $collapse.removeClass('guest-mode casino-mode');
            $collapse.addClass('investments-mode');
        });
        $('.nav-btn-investments').click(function () {
            $collapse.removeClass('guest-mode investments-mode');
            $collapse.addClass('casino-mode');
        });
        $collapse.addClass('guest-mode');
    });
})()