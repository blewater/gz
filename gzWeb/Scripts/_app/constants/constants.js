(function () {
    'use strict';

    // #region Routes
    var area = '/Mvc';
    //var areaCtrls = {
    //    guest: area + '/Guest',
    //    auth: area + '/Auth',
    //    games: area + '/Games',
    //    investments: area + '/Investments'
    //};

    // #region Keys
    var routeKeys = {
        home: 'home',
        transparency: 'transparency',
        about: 'about',

        login: 'login',
        register: 'register',

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
        auth: 'auth',
        games: 'games',
        investments: 'investments'
    };
    // #endregion

    // #region Guest
    var home = {
        key: routeKeys.home,
        group: groupKeys.guest,
        path: '/',
        ctrl: 'homeCtrl',
        tpl: '/Mvc/Guest/Home',
        title: 'Home'
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
        key: routeKeys.transparency,
        group: groupKeys.guest,
        path: '/about',
        ctrl: 'aboutCtrl',
        tpl: '/Mvc/Guest/About',
        title: 'About'
    };
    //var guest = {
    //    home: home,
    //    transparency: transparency,
    //    about: about
    //}
    // #endregion

    // #region Auth
    var login = {
        key: routeKeys.login,
        group: groupKeys.auth,
        path: '/login',
        ctrl: 'loginCtrl',
        tpl: '/Mvc/Auth/Login',
        title: 'Login'
    };
    var register = {
        key: routeKeys.register,
        group: groupKeys.auth,
        path: '/register',
        ctrl: 'registerCtrl',
        tpl: '/Mvc/Auth/Register',
        title: 'Register'
    };
    // TODO
    //login: '/Mvc/Account/Login',
    //verifyCode: '/Mvc/Account/VerifyCode',
    //register: '/Mvc/Account/Register',
    //confirmEmail: '/Mvc/Account/ConfirmEmail',
    //forgotPassword: '/Mvc/Account/ForgotPassword',
    //resetPassword: '/Mvc/Account/ResetPassword',
    //resetPasswordConfirmation: '/Mvc/Account/ResetPasswordConfirmation',
    //sendCode: '/Mvc/Account/SendCode',
    ////...
    //verifyPhoneNumber: '/Mvc/Manage/VerifyPhoneNumber',

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
    var auth = {
        login: login,
        register: register
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
        auth: auth,
        games: games,
        investments: investments
    };
    var routes = [
        home, transparency, about,
        login, register,
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

        area: area,
        routeKeys: routeKeys,
        groupKeys: groupKeys,
        routes: routes,
        routesGroups: routesGroups,

        spinners: {
            //sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
            sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
            sm_rel_green: { radius: 5, width: 2, length: 4, color: '#27A95C', position: 'relative', top: '0' },
            sm_rel: { radius: 5, width: 2, length: 4, position: 'relative', top: '0' },
        },
        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();