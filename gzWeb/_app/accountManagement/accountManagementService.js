(function() {
    "use strict";

    APP.factory("accountManagement", ['$compile', '$controller', '$templateRequest', 'helpers', 'auth', serviceFactory]);

    function serviceFactory($compile, $controller, $templateRequest, helpers, auth) {

        var _selectorId = '#state-content';

        // #region Account Management States
        var _states = { };
        //_states.deposit = {
        //    key: 'deposit',
        //    ctrl: 'depositCtrl',
        //    tpl: '_app/accountManagement/deposit.html',
        //    title: 'Deposit'
        //};
        //_states.withdraw = {
        //    key: 'withdraw',
        //    ctrl: 'withdrawCtrl',
        //    tpl: '_app/accountManagement/withdraw.html',
        //    title: 'Withdraw'
        //};
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
            action: _attachContent
        };
        _states.myProfile = {
            key: 'myProfile',
            ctrl: 'myProfileCtrl',
            tpl: '_app/accountManagement/myProfile.html',
            title: 'My Profile',
            icon: 'fa-user',
            action: _attachContent
        };
        _states.logout = {
            key: 'logout',
            title: 'Logout',
            icon: 'fa-sign-out',
            action: function () { auth.logout(); }
        };


        var _allStates = [];
        for (var key in _states) {
            _allStates.push(_states[key]);
        }
        _states.all = _allStates;
        // #endregion

        // #region Attach Content
        function _attachContent(state, scope) {
            var templateUrl = state.tpl;
            var ctrlId = state.ctrl;
            $templateRequest(helpers.ui.getTemplate(templateUrl)).then(function (html) {
                var $content = $(_selectorId);
                $content.contents().remove();
                $content.html(html);
                var ctrl = $controller(ctrlId, { $scope: scope });
                $content.children().data('$ngControllerController', ctrl);
                $compile($content.contents())(scope);
            });
        }
        // #endregion

        var _service = {
            states: _states,
            selectorId: _selectorId
        };
        return _service;
    };
})();