(function () {
    'use strict';

    // #region Routes
    var routes = {};

    // #region Guest
    routes.home = {
        key: 'home',
        path: '/',
        ctrl: 'homeCtrl',
        tpl: '/Mvc/Guest/Home',
        title: 'Home',
        reloadOnSearch: false
    };
    routes.transparency = {
        key: 'transparency',
        path: '/transparency',
        ctrl: 'transparencyCtrl',
        tpl: '/Mvc/Guest/Transparency',
        title: 'Transparency'
    };
    routes.about = {
        key: 'about',
        path: '/about',
        ctrl: 'aboutCtrl',
        tpl: '/Mvc/Guest/About',
        title: 'About'
    };
    routes.faq = {
        key: 'faq',
        path: '/faq',
        ctrl: 'faqCtrl',
        tpl: '/Mvc/Guest/FAQ',
        title: 'FAQ'
    };
    routes.playground = {
        key: 'playground',
        path: '/playground',
        ctrl: 'playgroundCtrl',
        tpl: '/Mvc/Guest/Playground',
        title: 'Playground'
    };
    // #endregion

    // #region Games
    routes.games = {
        key: 'games',
        path: '/games',
        ctrl: 'gamesCtrl',
        tpl: '/Mvc/Games/Games',
        title: 'Games'
    };
    // #endregion

    // #region Investments
    routes.summary = {
        key: 'summary',
        path: '/summary',
        ctrl: 'summaryCtrl',
        tpl: '/Mvc/Investments/Summary',
        title: 'Summary'
    };
    routes.portfolio = {
        key: 'portfolio',
        path: '/portfolio',
        ctrl: 'portfolioCtrl',
        tpl: '/Mvc/Investments/Portfolio',
        title: 'Portfolio'
    };
    routes.performance = {
        key: 'performance',
        path: '/performance',
        ctrl: 'performanceCtrl',
        tpl: '/Mvc/Investments/Performance',
        title: 'Performance'
    };
    routes.activity = {
        key: 'activity',
        path: '/activity',
        ctrl: 'activityCtrl',
        tpl: '/Mvc/Investments/Activity',
        title: 'Activity'
    };
    // #endregion

    var all = [];
    for (var key in routes)
        all.push(routes[key]);
    routes.all = all;
    // #endregion

    APP.constant("constants", {
        title: 'Greenzorro',
        version: 0.7,
        debugMode: true,
        html5Mode: true,

        webeocketApiUrl: 'wss://webapi-stage.everymatrix.com/v2',
        fallbackApiUrl: 'https://fb-webapi-stage.everymatrix.com',
        domainPrefix: 'http://www.greenzorro.com',
        emailVerificationUrl: 'localhost:63659/activate?key=',

        area: "/Mvc",
        routes: routes,

        spinners: {
            //sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
            sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
            sm_rel_green: { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'relative', top: '0' },
        },
        storageKeys: {
            randomSuffix: 'randomSuffix',
            authData: 'authData',
        },
        events: {
            SESSION_STATE_CHANGE: 'sessionStageChange',
            ACCOUNT_BALANCE_CHANGED: 'accountBalanceChanged',
            DEPOSIT_STATUS_CHANGED: 'depositStatusChanged',
            WITHDRAW_STATUS_CHANGED: 'withdrawStatusChanged'
        },
        reCaptchaPublicKey: '6LfPIgYTAAAAACEcTfYjFMr8y3GX6qYVLoK-2dML'

        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();