(function() {
    "use strict";

    APP.factory("accountManagement", ['$compile', '$controller', '$templateRequest', 'helpers', 'auth', '$animate', serviceFactory]);

    function serviceFactory($compile, $controller, $templateRequest, helpers, auth, $animate) {

        var _elementId = 'state-content';

        // #region Account Management States
        var _states = { };
        _states.deposit = {
            key: 'deposit',
            ctrl: 'depositCtrl',
            tpl: '_app/accountManagement/deposit.html',
            title: 'Deposit',
            type: 'button',
            btnType: 'plus',
            img: '../../Content/Images/plus_icon.svg',
            imgXs: '../../Content/Images/plus_icon_green.svg',
            action: angular.noop //_attachContent
        };
        //_states.depositPaymentMethods = {
        //    key: 'depositPaymentMethods',
        //    ctrl: 'depositPaymentMethodsCtrl',
        //    tpl: '_app/accountManagement/depositPaymentMethods.html',
        //    title: 'Deposit',
        //    type: 'button',
        //    btnType: 'plus',
        //    img: '../../Content/Images/plus_icon.svg',
        //    imgXs: '../../Content/Images/plus_icon_green.svg',
        //    action: _attachContent,
        //    showInMenu: true
        //};
        _states.withdraw = {
            key: 'withdraw',
            ctrl: 'withdrawCtrl',
            tpl: '_app/accountManagement/withdraw.html',
            title: 'Withdraw',
            type: 'button',
            btnType: 'minus',
            img: '../../Content/Images/minus_icon.svg',
            imgXs: '../../Content/Images/minus_icon_dgrey.svg',
            action: angular.noop //_attachContent
        };
        _states.pendingWithdrawals = {
            key: 'pendingWithdrawals',
            ctrl: 'pendingWithdrawalsCtrl',
            tpl: '_app/accountManagement/pendingWithdrawals.html',
            title: 'Pending Withdrawals',
            icon: 'fa-clock-o',
            action: _attachContent
        };
        _states.transactionHistory = {
            key: 'transactionHistory',
            ctrl: 'transactionHistoryCtrl',
            tpl: '_app/accountManagement/transactionHistory.html',
            title: 'Transaction History',
            icon: 'fa-exchange',
            action: _attachContent
        };
        _states.bonuses = {
            key: 'bonuses',
            ctrl: 'bonusesCtrl',
            tpl: '_app/accountManagement/bonuses.html',
            title: 'Bonuses',
            icon: 'fa-gift',
            action: angular.noop //_attachContent
        };
        _states.myProfile = {
            key: 'myProfile',
            ctrl: 'myProfileCtrl',
            tpl: '_app/accountManagement/myProfile.html',
            title: 'My Profile',
            icon: 'fa-user',
            action: _attachContent
        };
        _states.changePassword = {
            key: 'changePassword',
            ctrl: 'changePasswordCtrl',
            tpl: '_app/accountManagement/changePassword.html',
            title: 'Change Password',
            icon: 'fa-lock',
            action: angular.noop //_attachContent
        };
        _states.logout = {
            key: 'logout',
            title: 'Logout',
            icon: 'fa-sign-out',
            action: auth.logout
        };


        var _allStates = [];
        for (var key in _states) {
            _allStates.push(_states[key]);
        }
        _states.all = _allStates;
        // #endregion

        // #region Attach Content
        function _attachContent(state, callback) {
            helpers.ui.compile({
                selector: '#' + _elementId,
                templateUrl: state.tpl,
                controllerId: state.ctrl,
                callback: callback
            });
        }
        // #endregion

        var _service = {
            states: _states,
            elementId: _elementId
        };
        return _service;
    };
})();