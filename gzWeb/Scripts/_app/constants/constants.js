(function () {
    'use strict';

    // #region Roles
    var roles = {
        guest: 'guest',
        gamer: 'gamer',
        investor: 'investor',
        admin: 'admin'
    }
    // #endregion

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
        title: 'Games',
        roles: [roles.gamer]
    };
    // #endregion

    // #region Investments
    routes.summary = {
        key: 'summary',
        path: '/summary',
        ctrl: 'summaryCtrl',
        tpl: '/Mvc/Investments/Summary',
        title: 'Summary',
        roles: [roles.investor]
    };
    routes.portfolio = {
        key: 'portfolio',
        path: '/portfolio',
        ctrl: 'portfolioCtrl',
        tpl: '/Mvc/Investments/Portfolio',
        title: 'Portfolio',
        roles: [roles.investor]
    };
    routes.performance = {
        key: 'performance',
        path: '/performance',
        ctrl: 'performanceCtrl',
        tpl: '/Mvc/Investments/Performance',
        title: 'Performance',
        roles: [roles.investor]
    };
    routes.activity = {
        key: 'activity',
        path: '/activity',
        ctrl: 'activityCtrl',
        tpl: '/Mvc/Investments/Activity',
        title: 'Activity',
        roles: [roles.investor]
    };
    // #endregion

    // #region All
    var all = [];
    for (var key in routes)
        all.push(routes[key]);
    routes.all = all;
    // #endregion
    // #endregion

    APP.constant("constants", {
        title: 'Greenzorro',
        version: 0.7,
        debugMode: true,
        html5Mode: true,

        webSocketApiUrl: 'wss://webapi-stage.everymatrix.com/v2',
        fallbackApiUrl: 'https://fb-webapi-stage.everymatrix.com',
        domainPrefix: 'http://www.greenzorro.com',
        emailVerificationUrl: 'localhost:63659/activate?key=',

        area: "/Mvc",
        routes: routes,
        roles: roles,

        spinners: {
            //sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
            sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
            sm_rel_green: { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'relative', top: '0' },
        },
        storageKeys: {
            randomSuffix: 'randomSuffix',
            authData: 'authData',
            clientId: '$client_id$'
        },
        events: {
            ON_INIT: 'onInit',
            CONNECTION_INITIATED: 'connectionInitiated',
            AUTH_CHANGED: 'authChanged',
            SESSION_STATE_CHANGE: 'sessionStageChange',
            ACCOUNT_BALANCE_CHANGED: 'accountBalanceChanged',
            DEPOSIT_STATUS_CHANGED: 'depositStatusChanged',
            WITHDRAW_STATUS_CHANGED: 'withdrawStatusChanged'
        },

        reCaptchaPublicKey: '6Ld2ZB8TAAAAAFPviZAHanWXdifnC88VuM0DdsWO'
        //reCaptchaPublicKey: '6Ld5ZB8TAAAAAI1QlCbPCo-OnYi6EyR-lL2GrFyH'
        //reCaptchaPublicKey: '6LfPIgYTAAAAACEcTfYjFMr8y3GX6qYVLoK-2dML'

        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();