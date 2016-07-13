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

    // #region Categories
    var categories = {
        wandering: 'wandering',
        gaming: 'gaming',
        investing: 'investing',
    }
    // #endregion

    // #region Routes
    var routes = {};

    // #region Guest
    routes.home = {
        path: '/',
        ctrl: 'homeCtrl',
        tpl: '/Mvc/Guest/Home',
        title: 'Home',
        reloadOnSearch: false,
        category: categories.wandering
    };
    routes.transparency = {
        path: '/transparency',
        ctrl: 'transparencyCtrl',
        tpl: '/Mvc/Guest/Transparency',
        title: 'Transparency',
        category: categories.wandering
    };
    routes.about = {
        path: '/about',
        ctrl: 'aboutCtrl',
        tpl: '/Mvc/Guest/About',
        title: 'About',
        category: categories.wandering
    };
    routes.contact = {
        path: '/contact',
        ctrl: 'contactCtrl',
        tpl: '/Mvc/Guest/Contact',
        title: 'Contact',
        category: categories.wandering
    };
    routes.faq = {
        path: '/faq',
        ctrl: 'faqCtrl',
        tpl: '/Mvc/Guest/FAQ',
        title: 'FAQ',
        category: categories.wandering
    };
    routes.playground = {
        path: '/playground',
        ctrl: 'playgroundCtrl',
        tpl: '/Mvc/Guest/Playground',
        title: 'Playground',
        category: categories.wandering
    };
    // #endregion

    // #region Games
    routes.games = {
        path: '/games',
        ctrl: 'gamesCtrl',
        tpl: '/Mvc/Games/Games',
        title: 'Games',
        roles: [roles.gamer],
        category: categories.gaming
    };
    routes.game = {
        path: '/game/:slug',
        ctrl: 'gameCtrl',
        tpl: '/Mvc/Games/Game',
        title: 'Games',
        roles: [roles.gamer],
        category: categories.gaming
    };
    // #endregion

    // #region Investments
    routes.summary = {
        path: '/summary',
        ctrl: 'summaryCtrl',
        tpl: '/Mvc/Investments/Summary',
        title: 'Summary',
        roles: [roles.investor],
        category: categories.investing
    };
    routes.portfolio = {
        path: '/portfolio',
        ctrl: 'portfolioCtrl',
        tpl: '/Mvc/Investments/Portfolio',
        title: 'Portfolio',
        roles: [roles.investor],
        category: categories.investing
    };
    routes.performance = {
        path: '/performance',
        ctrl: 'performanceCtrl',
        tpl: '/Mvc/Investments/Performance',
        title: 'Performance',
        roles: [roles.investor],
        category: categories.investing
    };
    routes.activity = {
        path: '/activity',
        ctrl: 'activityCtrl',
        tpl: '/Mvc/Investments/Activity',
        title: 'Activity',
        roles: [roles.investor],
        category: categories.investing
    };
    // #endregion

    // #region All
    var all = [];
    for (var key in routes) {
        if (routes[key].roles === undefined)
            routes[key].roles = [roles.guest];
        if (routes[key].category === undefined)
            routes[key].category = categories.wandering;
        if (routes[key].reloadOnSearch === undefined)
            routes[key].reloadOnSearch = true;
        all.push(routes[key]);
    }
    routes.all = all;
    // #endregion
    // #endregion

    APP.constant("constants", {
        title: 'greenzorro',
        //version: 0.7,
        //debugMode: true,
        html5Mode: true,
        
        emailVerificationUrl: 'localhost:63659/activate?key=',

        area: "/Mvc",
        routes: routes,
        categories: categories,
        roles: roles,

        spinners: {
            //sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
            sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
            sm_rel_green: { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'relative', top: '0' },
            xs_rel_white: { radius: 4, width: 2, length: 3, color: '#fff', position: 'relative', top: '0' },
            xs_rel_green: { radius: 4, width: 2, length: 3, color: '#27A95C', position: 'relative', top: '0' },
            xs_abs_green: { radius: 4, width: 2, length: 3, color: '#27A95C', position: 'absolute', top: '50%' },
        },
        storageKeys: {
            version: 'gz_version',
            debug: 'gz_debug',
            randomSuffix: 'gz_randomSuffix',
            authData: 'gz_authData',
            clientId: 'gz_$client_id$',
            reCaptchaPublicKey: 'gz_reCaptchaPublicKey'
        },
        events: {
            ON_INIT: 'onInit',
            CONNECTION_INITIATED: 'connectionInitiated',
            AUTH_CHANGED: 'authChanged',
            SESSION_STATE_CHANGE: 'sessionStageChange',
            ACCOUNT_BALANCE_CHANGED: 'accountBalanceChanged',
            REQUEST_ACCOUNT_BALANCE: 'requestAccountBalance',
            DEPOSIT_STATUS_CHANGED: 'depositStatusChanged',
            WITHDRAW_STATUS_CHANGED: 'withdrawStatusChanged'
        }

        //reCaptchaPublicKey: '6Ld2ZB8TAAAAAFPviZAHanWXdifnC88VuM0DdsWO'
        //reCaptchaPublicKey: '6Ld5ZB8TAAAAAI1QlCbPCo-OnYi6EyR-lL2GrFyH'
        //reCaptchaPublicKey: '6LfPIgYTAAAAACEcTfYjFMr8y3GX6qYVLoK-2dML'

        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();