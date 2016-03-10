(function () {
    'use strict';

    // #region Routes
    var area = '/Mvc';
    //var areaCtrls = {
    //    guest: area + '/Guest',
    //    auth: area + '/Auth',
    //    casino: area + '/Casino',
    //    investments: area + '/Investments'
    //};

    // #region Keys
    var routeKeys = {
        home: 'home',
        transparency: 'transparency',
        about: 'about',

        login: 'login',
        register: 'register',

        casino: 'casino',
        casino2: 'casino2',
        casino3: 'casino3',
        casino4: 'casino4',

        summary: 'summary',
        portfolio: 'portfolio',
        performance: 'performance',
        activity: 'activity',
    };

    var groupKeys = {
        guest: 'guest',
        auth: 'auth',
        casino: 'casino',
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

    // #region Casino
    var casino1 = {
        key: routeKeys.casino,
        group: groupKeys.casino,
        path: '/casino',
        ctrl: 'casinoIndexCtrl',
        tpl: '/Mvc/Casino/Index',
        title: 'Casino'
    };
    var casino2 = {
        key: routeKeys.casino2,
        group: groupKeys.casino,
        path: '/casino2',
        ctrl: 'casinoPage2Ctrl',
        tpl: '/Mvc/Casino/Page2',
        title: 'Casino 2'
    };
    var casino3 = {
        key: routeKeys.casino3,
        group: groupKeys.casino,
        path: '/casino3',
        ctrl: 'casinoPage3Ctrl',
        tpl: '/Mvc/Casino/Page3',
        title: 'Casino 3'
    };
    var casino4 = {
        key: routeKeys.casino4,
        group: groupKeys.casino,
        path: '/casino4',
        ctrl: 'casinoPage4Ctrl',
        tpl: '/Mvc/Casino/Page4',
        title: 'Casino 4'
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
    var casino = {
        casino1: casino1,
        casino2: casino2,
        casino3: casino3,
        casino4: casino4
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
        casino: casino,
        investments: investments
    };
    var routes = [
        home, transparency, about,
        login, register,
        casino1, casino2, casino3, casino4,
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

        //spinners: {
        //    sm_abs_black: { radius: 5, width: 2, length: 4, color: '#000' },
        //    sm_rel_white: { radius: 5, width: 2, length: 4, color: '#fff', position: 'relative', top: '0' },
        //},
        //msgs: {
        //    required: 'The field is required!',
        //    invalidEmail: 'Invalid e-mail format!'
        //},
    });
})();