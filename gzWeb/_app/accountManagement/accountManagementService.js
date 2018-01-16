﻿(function() {
    "use strict";

    APP.factory("accountManagement", ['$compile', '$controller', '$templateRequest', '$filter', 'helpers', 'auth', 'message', 'emBanking', '$location', serviceFactory]);

    function serviceFactory($compile, $controller, $templateRequest, $filter, helpers, auth, message, emBanking, $location) {

        var _elementId = 'state-content';

        // #region Account Management States
        var _states = { };
        _states.depositPaymentMethods = {
            key: 'deposit',
            ctrl: 'depositPaymentMethodsCtrl',
            tpl: '_app/accountManagement/depositPaymentMethods.html',
            title: 'Deposit',
            type: 'button',
            btnType: 'plus',
            img: '../../Content/Images/plus_icon.svg',
            imgXs: '../../Content/Images/plus_icon_green.svg',
            iconMenu: 'fa-plus-square',
            action: _attachContent,
            showInMenu: true,
            route: 'deposit'
        };
        _states.deposit = {
            key: 'deposit',
            ctrl: 'depositCtrl',
            tpl: '_app/accountManagement/deposit.html',
            title: 'Deposit',
            type: 'button',
            btnType: 'plus',
            img: '../../Content/Images/plus_icon.svg',
            imgXs: '../../Content/Images/plus_icon_green.svg',
            showInMenu: false,
            action: _attachContent
        };
        _states.withdrawPaymentMethods = {
            key: 'withdraw',
            ctrl: 'withdrawPaymentMethodsCtrl',
            tpl: '_app/accountManagement/withdrawPaymentMethods.html',
            title: 'Withdraw',
            type: 'button',
            btnType: 'minus',
            img: '../../Content/Images/minus_icon.svg',
            imgXs: '../../Content/Images/minus_icon_dgrey.svg',
            iconMenu: 'fa-minus-square',
            showInMenu: true,
            action: _attachContent,
            route: 'withdraw'
        };
        _states.withdraw = {
            key: 'withdraw',
            ctrl: 'withdrawCtrl',
            tpl: '_app/accountManagement/withdraw.html',
            //ctrl: 'withdrawDynamicCtrl',
            //tpl: '_app/accountManagement/withdrawDynamic.html',
            title: 'Withdraw',
            type: 'button',
            btnType: 'minus',
            img: '../../Content/Images/minus_icon.svg',
            imgXs: '../../Content/Images/minus_icon_dgrey.svg',
            showInMenu: false,
            action: _attachContent
        };
        _states.pendingWithdrawals = {
            key: 'pendingWithdrawals',
            ctrl: 'pendingWithdrawalsCtrl',
            tpl: '_app/accountManagement/pendingWithdrawals.html',
            title: 'Pending Withdrawals',
            icon: 'fa-clock-o',
            showInMenu: true,
            action: _attachContent,
            route: 'pending-withdrawals'
        };
        _states.transactionHistory = {
            key: 'transactionHistory',
            ctrl: 'transactionHistoryCtrl',
            tpl: '_app/accountManagement/transactionHistory.html',
            title: 'Transaction History',
            icon: 'fa-exchange',
            showInMenu: true,
            action: _attachContent,
            route: 'transaction-history'
        };
        _states.bonuses = {
            key: 'bonuses',
            ctrl: 'bonusesCtrl',
            tpl: '_app/accountManagement/bonuses.html',
            title: 'Bonuses',
            icon: 'fa-gift',
            showInMenu: true,
            action: _attachContent,
            route: 'bonuses'
        };
        _states.responsibleGaming = {
            key: 'responsibleGaming',
            ctrl: 'responsibleGamingCtrl',
            tpl: '_app/accountManagement/responsibleGaming.html',
            title: 'Responsible Gaming',
            icon: 'fa-ban',
            showInMenu: true,
            action: _attachContent,
            route: 'responsible-gaming'
        };
        _states.myProfile = {
            key: 'myProfile',
            ctrl: 'myProfileCtrl',
            tpl: '_app/accountManagement/myProfile.html',
            title: 'My Profile',
            icon: 'fa-user',
            showInMenu: true,
            action: _attachContent,
            route: 'my-profile'
        };
        _states.changePassword = {
            key: 'changePassword',
            ctrl: 'changePasswordCtrl',
            tpl: '_app/accountManagement/changePassword.html',
            title: 'Change Password',
            icon: 'fa-lock',
            showInMenu: true,
            action: _attachContent,
            route: 'change-password'
        };
        _states.logout = {
            key: 'logout',
            title: 'Logout',
            icon: 'fa-sign-out',
            showInMenu: true,
            action: function () { auth.logout(); }
        };
        
        var _allStates = [];
        for (var key in _states) {
            _allStates.push(_states[key]);
        }

        _states.all = _allStates;
        _states.menu = $filter('filter')(_allStates, { showInMenu: true });
        // #endregion

        // #region Attach Content
        function _attachContent(state, scope, params, callback) {
            $location.search({ open: state.route });
            helpers.ui.compile({
                selector: '#' + _elementId,
                templateUrl: state.tpl,
                controllerId: state.ctrl,
                scope: scope,
                params: params,
                callback: callback
            });
        }
        // #endregion

        // #region Open
        function open(state) {
            if (angular.isUndefined(state))
                state = _states.menu[0];
            message.open({
                nsType: 'modal',
                nsSize: '1000px',
                nsTemplate: '_app/accountManagement/accountManagement.html',
                nsCtrl: 'accountManagementCtrl',
                nsStatic: true,
                nsRoute: state.route,
                nsParams: { state: state }
            });
        };
        // #endregion

        var _service = {
            states: _states,
            elementId: _elementId,
            open: open
        };
        return _service;
    };
})();