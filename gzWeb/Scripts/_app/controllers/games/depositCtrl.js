(function() {
    "use strict";

    var ctrlId = "depositCtrl";
    APP.controller(ctrlId, ["$scope", "emWamp", "emBanking", ctrlFactory]);

    function ctrlFactory($scope, emWamp, emBanking) {

        $scope.model = {
            paymentMethod: emBanking.PaymentMethodCode.VISA
        };

        $scope.sessionInfo = {};
        $scope.gamingAccounts = [];
        $scope.paymentMethods = [];

        function logError(error) {
            console.log(error);
        };

        $scope.onPaymentMethodChanged = function () {
            emBanking.getPaymentMethodCfg($scope.model.paymentMethod)
                .then(function(result) {
                    // TODO: Display the appropriate UI
                    // TODO: ...
                    $scope.model.fields = result.fields;
                }, logError);
        };

        // Step 1 - prepareDiposit
        $scope.prepareDiposit = function() {
            // TODO: validate input fields
            emBanking.prepare({
                    paymentMethodCode: $scope.model.paymentMethod,
                    fields: $scope.model.fields
                })
                .then(function(result) {
                        scope.model.pid = result.pid;

                        if (result.status === "setup") {
                            // TODO: show confirmation page
                        } else if (result.status === "redirection") {
                            // TODO: redirection ...
                        } else {
                            // TODO: log error ???
                        }
                    },
                    logError);
        };

        // Step 2 - confirmDiposit
        $scope.confirmDiposit = function () {
            // TODO: validate input fields
            emBanking.confirm($scope.model.pid)
                .then(function(confirmResult) {
                        if (confirmResult.status === "success") {
                            emBanking.getTransactionInfo()
                                .then(function(txResult) {
                                        if (txResult.status === "success") {
                                            // TODO: show receipt page ...
                                        } else if (txResult.status === "incomplete") {
                                            // TODO: show transaction is not completed
                                        } else if (txResult.status === "pending") {
                                            // TODO: show transaction is pending
                                        } else if (txResult.status === "error") {
                                            // TODO: show error
                                        }
                                    },
                                    logError);
                        } else if (confirmResult.status === "redirection") {
                            // TODO: redirection ...
                        } else if (confirmResult.status === "instructions") {
                            // TODO: instructions ...
                        } else {
                            // TODO: log error ???
                        }
                    },
                    logError);
        };
        
        function getSupportedPaymentMethods() {
            emBanking.getSupportedPaymentMethods($scope.sessionInfo.userCountry, $scope.sessionInfo.currency)
                .then(function(paymentMethods) {
                        $scope.paymentMethods = paymentMethods;
                    },
                    logError);
        };

        function getGamingAccounts() {
            emBanking.getGamingAccounts(false, false)
                .then(function(result) {
                        $scope.gamingAccounts = result.accounts;
                    },
                    logError);
        }

        function init() {
            emWamp.getSessionInfo()
                .then(function(sessionInfo) {
                        $scope.sessionInfo = sessionInfo;
                        getGamingAccounts();
                        getSupportedPaymentMethods();
                    },
                    logError);
        };

        init();
    };

})();