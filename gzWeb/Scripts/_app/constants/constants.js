(function () {
    'use strict';

    // #region Routes
    var area = '/Mvc';

    // #region Keys
    var routeKeys = {
        home: 'home',
        transparency: 'transparency',
        about: 'about',
        faq: 'faq',
        playground: 'playground',

        games1: 'games1',
        games2: 'games2',
        games3: 'games3',
        games4: 'games4',

        summary: 'summary',
        portfolio: 'portfolio',
        performance: 'performance',
        activity: 'activity',
    };

    var groupKeys = {
        guest: 'guest',
        games: 'games',
        investments: 'investments'
    };
    // #endregion

    // #region Guest
    var home = {
        key: routeKeys.home,
        //group: groupKeys.guest,
        path: '/',
        ctrl: 'homeCtrl',
        tpl: '/Mvc/Guest/Home',
        title: 'Home',
        reloadOnSearch: false
    };
    var transparency = {
        key: routeKeys.transparency,
        group: groupKeys.guest,
        path: '/transparency',
        ctrl: 'transparencyCtrl',
        tpl: '/Mvc/Guest/Transparency',
        title: 'Transparency'
    };
    var about = {
        key: routeKeys.about,
        group: groupKeys.guest,
        path: '/about',
        ctrl: 'aboutCtrl',
        tpl: '/Mvc/Guest/About',
        title: 'About'
    };
    var faq = {
        key: routeKeys.faq,
        //group: groupKeys.guest,
        path: '/faq',
        ctrl: 'faqCtrl',
        tpl: '/Mvc/Guest/FAQ',
        title: 'FAQ'
    };
    var playground = {
        key: routeKeys.playground,
        //group: groupKeys.guest,
        path: '/playground',
        ctrl: 'playgroundCtrl',
        tpl: '/Mvc/Guest/Playground',
        title: 'Playground'
    };
    // #endregion

    // #region Games
    var games1 = {
        key: routeKeys.games1,
        group: groupKeys.games,
        path: '/games',
        ctrl: 'games1Ctrl',
        tpl: '/Mvc/Games/Page1',
        title: 'Games 1'
    };
    var games2 = {
        key: routeKeys.games2,
        group: groupKeys.games,
        path: '/games2',
        ctrl: 'games2Ctrl',
        tpl: '/Mvc/Games/Page2',
        title: 'Games 2'
    };
    var games3 = {
        key: routeKeys.games3,
        group: groupKeys.games,
        path: '/games3',
        ctrl: 'games3Ctrl',
        tpl: '/Mvc/Games/Page3',
        title: 'Games 3'
    };
    var games4 = {
        key: routeKeys.games4,
        group: groupKeys.games,
        path: '/games4',
        ctrl: 'games4Ctrl',
        tpl: '/Mvc/Games/Page4',
        title: 'Games 4'
    };
    // #endregion

    // #region Investments
    var summary = {
        key: routeKeys.summary,
        group: groupKeys.investments,
        path: '/summary',
        ctrl: 'summaryCtrl',
        tpl: '/Mvc/Investments/Summary',
        title: 'Summary'
    };
    var portfolio = {
        key: routeKeys.portfolio,
        group: groupKeys.investments,
        path: '/portfolio',
        ctrl: 'portfolioCtrl',
        tpl: '/Mvc/Investments/Portfolio',
        title: 'Portfolio'
    };
    var performance = {
        key: routeKeys.performance,
        group: groupKeys.investments,
        path: '/performance',
        ctrl: 'performanceCtrl',
        tpl: '/Mvc/Investments/Performance',
        title: 'Performance'
    };
    var activity = {
        key: routeKeys.activity,
        group: groupKeys.investments,
        path: '/activity',
        ctrl: 'activityCtrl',
        tpl: '/Mvc/Investments/Activity',
        title: 'Activity'
    };
    // #endregion

    // #region Groups
    var guest = {
        home: home,
        transparency: transparency,
        about: about
    };
    var games = {
        games1: games1,
        games2: games2,
        games3: games3,
        games4: games4
    };
    var investments = {
        summary: summary,
        portfolio: portfolio,
        performance: performance,
        activity: activity
    };
    var routesGroups = {
        guest: guest,
        //auth: auth,
        games: games,
        investments: investments
    };
    var routes = [
        home, transparency, about, faq, playground,
        //login, register,
        games1, games2, games3, games4,
        summary, portfolio, performance, activity
    ];
    // #endregion
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

        area: area,
        routeKeys: routeKeys,
        groupKeys: groupKeys,
        routes: routes,
        routesGroups: routesGroups,

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
            SESSION_STATE_CHANGE: 'sessionStageChange'
        },
        reCaptchaPublicKey: '6LfPIgYTAAAAACEcTfYjFMr8y3GX6qYVLoK-2dML'

        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();