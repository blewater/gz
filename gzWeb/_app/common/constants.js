(function () {
    'use strict';

    // #region ActionTypes
    var actionTypes = {
        url: 0,
        page: 1,
        game: 2
    }
    // #endregion

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
        tpl: '_app/guest/home.html',
        title: 'Home',
        reloadOnSearch: false,
        category: categories.wandering
    };
    routes.transparency = {
        path: '/transparency',
        ctrl: 'transparencyCtrl',
        tpl: '_app/guest/transparency.html',
        title: 'Transparency',
        category: categories.wandering
    };
    routes.about = {
        path: '/about',
        ctrl: 'aboutCtrl',
        tpl: '_app/guest/about.html',
        title: 'About',
        category: categories.wandering
    };
    //routes.contact = {
    //    path: '/contact',
    //    ctrl: 'contactCtrl',
    //    tpl: '/Mvc/Guest/Contact',
    //    title: 'Contact',
    //    category: categories.wandering
    //};
    routes.faq = {
        path: '/faq',
        ctrl: 'faqCtrl',
        tpl: '_app/guest/faq.html',
        title: 'FAQ',
        category: categories.wandering
    };
    routes.help = {
        path: '/help',
        ctrl: 'helpCtrl',
        tpl: '_app/guest/help.html',
        title: 'Help',
        category: categories.wandering
    };
    routes.privacy = {
        path: '/privacy',
        ctrl: 'privacyCtrl',
        tpl: '_app/guest/privacy.html',
        title: 'Privacy Policy',
        category: categories.wandering
    };
    routes.terms = {
        path: '/terms',
        ctrl: 'termsCtrl',
        tpl: '_app/guest/terms.html',
        title: 'Terms & Conditions',
        category: categories.wandering
    };
    routes.playground = {
        path: '/playground',
        ctrl: 'playgroundCtrl',
        tpl: '_app/guest/playground.html',
        title: 'Playground',
        category: categories.wandering
    };
    routes.terms = {
        path: '/terms',
        ctrl: 'termsCtrl',
        tpl: '_app/guest/terms.html',
        title: 'Terms & Conditions',
        category: categories.wandering
    };
    routes.responsibleGambling = {
        path: '/responsible-gambling',
        ctrl: 'responsibleGamblingCtrl',
        tpl: '_app/guest/responsibleGambling.html',
        title: 'Responsible Gambling',
        category: categories.wandering
    };

    routes.promotion = {
        path: '/promotions/:code',
        ctrl: 'promotionCtrl',
        tpl: '_app/promotions/promotion.html',
        title: 'Promotions',
        roles: [roles.guest],
        category: categories.wandering
    };
    routes.promotions = {
        path: '/promotions',
        ctrl: 'promotionsCtrl',
        tpl: '_app/promotions/promotions.html',
        title: 'Promotions',
        roles: [roles.guest],
        category: categories.wandering
    };
    // #endregion

    // #region Games
    routes.games = {
        path: '/casino',
        ctrl: 'gamesCtrl',
        tpl: '_app/games/games.html',
        title: 'Casino',
        reloadOnSearch: false,
        roles: [roles.gamer],
        category: categories.gaming
    };
    routes.game = {
        path: '/casino/:slug',
        ctrl: 'gameCtrl',
        tpl: '_app/games/game.html',
        title: 'Casino',
        reloadOnSearch: false,
        roles: [roles.gamer],
        category: categories.gaming
    };
    // #endregion

    // #region Investments
    routes.summary = {
        path: '/summary',
        ctrl: 'summaryCtrl',
        tpl: '_app/investments/summary.html',
        title: 'Summary',
        roles: [roles.investor],
        category: categories.investing
    };
    routes.portfolio = {
        path: '/portfolio',
        ctrl: 'portfolioCtrl',
        tpl: '_app/investments/portfolio.html',
        title: 'Portfolio',
        roles: [roles.investor],
        category: categories.investing
    };
    routes.performance = {
        path: '/performance',
        ctrl: 'performanceCtrl',
        tpl: '_app/investments/performance.html',
        title: 'Performance',
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
        html5Mode: true,
        
        emailVerificationUrl: 'localhost:63659/activate?key=',

        area: "/Mvc",
        routes: routes,
        categories: categories,
        roles: roles,
        carouselActionTypes: actionTypes,

        spinners: {
            //sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
            md_abs_white: { radius: 8, width: 3, length: 6, color: '#fff', position: 'absolute', top: '50%' },
            sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
            sm_rel_green: { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'relative', top: '0' },
            sm_abs_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'absolute', top: '50%' },
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
            CHAT_LOADED: 'chatLoaded',
            SESSION_STATE_CHANGE: 'sessionStageChange',
            ACCOUNT_BALANCE_CHANGED: 'accountBalanceChanged',
            REQUEST_ACCOUNT_BALANCE: 'requestAccountBalance',
            DEPOSIT_STATUS_CHANGED: 'depositStatusChanged',
            WITHDRAW_STATUS_CHANGED: 'withdrawStatusChanged'
        }
    });
})();