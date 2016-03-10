(function() {
    "use strict";

    $(function () {
        var $collapse = $('#header-collapse-section');
        $('.nav-btn-login').click(function () {
            $collapse.removeClass('guest-mode investments-mode');
            $collapse.addClass('games-mode');
        });
        $('.nav-btn-games').click(function () {
            $collapse.removeClass('guest-mode games-mode');
            $collapse.addClass('investments-mode');
        });
        $('.nav-btn-investments').click(function () {
            $collapse.removeClass('guest-mode investments-mode');
            $collapse.addClass('games-mode');
        });
        $collapse.addClass('guest-mode');
    });
})()