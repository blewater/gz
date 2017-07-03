(function () {
    'use strict';

    APP.factory('modals', ['message', '$location', 'auth', 'constants', '$filter', 'accountManagement', serviceFactory]);
    function serviceFactory(message, $location, auth, constants, $filter, accountManagement) {
        var _modals = {};
        _modals.login = {
            nsType: 'modal',
            nsSize: '600px',
            nsTemplate: '_app/account/login.html',
            nsCtrl: 'loginCtrl',
            nsStatic: true,
            nsRoute: 'login'
        };
        _modals.register = {
            nsType: 'modal',
            nsSize: '600px',
            nsTemplate: '_app/account/registerAccount.html',
            nsCtrl: 'registerAccountCtrl',
            nsStatic: true,
            nsRoute: 'signup'
        };
        _modals.forgotPassword = {
            nsType: 'modal',
            nsSize: '600px',
            nsTemplate: '_app/account/forgotPassword.html',
            nsCtrl: 'forgotPasswordCtrl',
            nsStatic: true,
            nsRoute: 'forgot-password'
        };
        var _allModals = [];
        for (var key in _modals) {
            _allModals.push(_modals[key]);
        }
        _modals.all = _allModals;

        var _allStates= [];
        for (var key in accountManagement.states) {
            _allStates.push(accountManagement.states[key]);
        }


        var service = {
            open: open,
            login: login,
            register: register,
            forgotPassword: forgotPassword
        };
        return service;

        function open(route) {
            if (route) {
                var anonymousModal = $filter('filter')(_modals.all, { nsRoute: route })[0];
                var authorizedModal = auth.data.isGamer && $filter('filter')(_allStates, { route: route })[0];
                if (anonymousModal)
                    return message.open(anonymousModal);
                else if (authorizedModal)
                    return accountManagement.open(authorizedModal);
                else {
                    $location.search({open: null});
                    return undefined;
                }
            }
            return undefined;
        };
        function login(method) {
            return method(_modals.login);
        };
        function register(method) {
            return method(_modals.register);
        };
        function forgotPassword(method) {
            return method(_modals.forgotPassword);
        };
    };
})();