(function () {
    'use strict';
    var ctrlId = 'summaryCtrl';
    APP.controller(ctrlId, ['$scope', '$controller', 'api', 'message', '$location', 'constants', '$filter', '$sce', 'iso4217', 'ngIntroService', ctrlFactory]);
    function ctrlFactory($scope, $controller, api, message, $location, constants, $filter, $sce, iso4217, ngIntroService) {
        $controller('authCtrl', { $scope: $scope });

        $scope.spinnerWhite = constants.spinners.sm_abs_white;
        var sellingValuesFetched = false;

        $scope.openVintages = function (title, vintages, withdrawMode, alreadyWithdrawn) {
            return message.modal(title, {
                nsSize: '700px',
                nsTemplate: '_app/investments/summaryVintages.html',
                nsCtrl: 'summaryVintagesCtrl',
                nsStatic: withdrawMode,
                nsParams: {
                    vintages: vintages,
                    withdrawMode: withdrawMode,
                    currency: $scope.currency,
                    alreadyWithdrawn: alreadyWithdrawn
                    //getInvestmentText: $scope.getInvestmentText
                }
            });
        };

        $scope.showAllVintages = function () {
            window.appInsights.trackEvent("OPEN SHOW VINTAGES");
            $scope.openVintages('Vintages history', $scope.vintages, false);
        };

        $scope.withdraw = function () {
            if (!sellingValuesFetched) {
                $scope.fetchingSellingValues = true;
                api.call(function () {
                    return api.getVintagesWithSellingValues($scope.vintages);
                }, function (getVintagesResponse) {
                    sellingValuesFetched = true;
                    $scope.fetchingSellingValues = false;
                    $scope.vintages = processVintages(getVintagesResponse.Result.Vintages);
                    $scope.alreadyWithdrawn = getVintagesResponse.Result.WithdrawnAmount;
                    withdrawVintages();
                });
            }
            else
                withdrawVintages();
        };

        function withdrawVintages() {
            window.appInsights.trackEvent("OPEN WITHDRAW VINTAGES");
            var withdrawableVintages = $filter('omit')($scope.vintages, function (v) { return v.InvestmentAmount === 0; });
            var promise = $scope.openVintages('Available funds for withdrawal', withdrawableVintages, true, $scope.alreadyWithdrawn);
            promise.then(function (updatedVintages) {
                api.call(function () {
                    window.appInsights.trackEvent("ACTUAL WITHDRAW VINTAGES");
                    return api.withdrawVintages(updatedVintages);
                }, function (withdrawResponse) {
                    $scope.vintages = processVintages(withdrawResponse.Result);
                    var netProceeds = getVintageSoldNetProceeds(withdrawResponse.Result);
                    message.success("To be credited into your casino wallet (" + netProceeds.toString() + " " + $scope.currency + " pending withdrawal).");
                }, {
                    rejectFn: function() {
                        var selectedCount = $filter('where')(updatedVintages, { 'Selected': true }).length;
                        var failureMsg =
                            "Our attempt to sell your investment vintage" +
                            (selectedCount === 1 ? "" : "s") +
                            " has not been successful. We apologize for the inconvenience. Please try again later.";
                        message.error(failureMsg);
                    }
                });
            }, function(error) {
                for (var i = 0; i < $scope.vintages.length; i++)
                    $scope.vintages[i].Selected = false;
            });
        }

        $scope.backToGames = function() {
            window.appInsights.trackEvent("GOTO GAMES", { from: "SUMMARY" });
            $location.path(constants.routes.games.path).search({});
        };

        $scope.intoPortfolio = function () {
            window.appInsights.trackEvent("GOTO PORTOFOLIO", { from: "SUMMARY" });
            $location.path(constants.routes.portfolio.path).search({});
        };

        $scope.getInvestmentText = function(investmentAmount) {
            if (investmentAmount === 0)
                return "N/A";
            else if (investmentAmount > 0 && investmentAmount < 1)
                return iso4217.getCurrencyByCode($scope.currency).symbol + "<1";
            else
                return $filter('isoCurrency')(investmentAmount, $scope.currency, 0);
        }
        function processVintages(vintages) {
            var mappedVintages = $filter('map')(vintages, function (v) {
                v.Year = parseInt(v.YearMonthStr.slice(0, 4));
                v.Month = parseInt(v.YearMonthStr.slice(-2));
                v.Date = new Date(v.Year, v.Month - 1);
                v.InvestmentText = $scope.getInvestmentText(v.InvestmentAmount);
                return v;
            });
            var ordered = $filter('orderBy')(mappedVintages, 'Date', true);
            return ordered;
        }
        function getVintageSoldNetProceeds(vintages) {
            var totalNetProceeds = vintages.reduce(function (prevVal, v) {
                var netAmount = v.Selected ? prevVal + (v.SellingValue - v.SoldFees) : prevVal;
                return netAmount;
            }, 0);
            return totalNetProceeds;
        }

        $scope.toggleInvestmentHistory = function () {
            $scope.investmentHistoryExpanded = !$scope.investmentHistoryExpanded;
        };
        $scope.toggleGamingActivities = function () {
            window.appInsights.trackEvent("INVESTMENT " + ($scope.gamingActivitiesExpanded ? "CLOSE" : "OPEN") + " GAMING ACTIVITIES");
            $scope.gamingActivitiesExpanded = !$scope.gamingActivitiesExpanded;
        };

        function round(amount, decimals) {
            var pow = Math.pow(10, decimals);
            return Math.round(amount * pow) / pow;
        }

        function loadSummaryData() {
            api.call(function () {
                return api.getSummaryData();
            }, function (response) {
                $scope.model = response.Result;
                var statusAsOfLocal = moment.utc($scope.model.StatusAsOf).toDate();
                $scope.model.StatusAsOfLocalDate = $filter('date')(statusAsOfLocal, 'MMMM d');
                $scope.model.StatusAsOfLocalTime = $filter('date')(statusAsOfLocal, 'h:mm a');
                var nextInvestmentOnLocal = moment.utc($scope.model.NextInvestmentOn).toDate();
                $scope.model.NextInvestmentOnLocalDate = $filter('date')(nextInvestmentOnLocal, 'MMMM d');
                $scope.model.NextInvestmentOnLocalTime = $filter('date')(nextInvestmentOnLocal, 'h:mm a');
                $scope.model.CurrentMonth = $filter('date')(moment.utc().toDate(), 'MMMM');
                var utcNow = moment.utc().toDate();
                var startOfMonth = new Date(utcNow.getFullYear(), utcNow.getMonth(), 1);
                var todayDate = utcNow.getDate();
                $scope.model.GamingActivitiesRange =
                    $filter('ordinalDate')(startOfMonth, 'MMMM d') +
                    (todayDate === 1 ? '' : (' - ' + $filter('ordinalDate')(utcNow, 'd')));
                $scope.model.OkToWithdraw = false;

                $scope.model.InvestmentsBalance = Math.round($scope.model.InvestmentsBalance);
                $scope.model.TotalInvestments = Math.round($scope.model.TotalInvestments);
                $scope.model.TotalInvestmentsReturns = $scope.model.InvestmentsBalance - $scope.model.TotalInvestments;

                $scope.model.BegGmBalance = round($scope.model.BegGmBalance, 1);
                $scope.model.Deposits = round($scope.model.Deposits, 1);
                $scope.model.Withdrawals = round($scope.model.Withdrawals, 1);
                $scope.model.EndGmBalance = round($scope.model.EndGmBalance, 1);
                $scope.model.GmGainLoss = round($scope.model.EndGmBalance - $scope.model.BegGmBalance - $scope.model.Deposits + $scope.model.Withdrawals, 1);

                $scope.vintages = processVintages($scope.model.Vintages);
                setIntroOptionsForUsers($scope.vintages.length, $scope.model.InvestmentsBalance, $scope.model.GmGainLoss);
            });            
        }

        function loadAuthData() {
            $scope.currency = $scope._authData.currency;
        }

        function setIntroNewUsers() {
            var introOptionsNewUsers = {
                steps: [
                    {
                        element: "#toBeInvested",
                        intro: "Play casino games and 50% of your net loss will be credited into your investment balance."
                    }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                exitOnEsc: true,
                skipLabel: 'Exit',
                doneLabel: 'Got it!',
                tooltipPosition: 'auto'
            }
            ngIntroService.setOptions(introOptionsNewUsers);
            ngIntroService.start();
        }

        function setIntroOtherUsers() {
            var otherUsersOptions = {
                steps: [
                    {
                        element: "#toBeInvested",
                        intro:
                            "Play casino games and 50% of your net loss will be credited into your investment balance."
        },
                    {
                        element: "#currentBalance",
                        intro: "Did you know that your balance is increasing the more you play?"
                    }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                exitOnEsc: true,
                nextLabel: '<strong>Next Tip</strong>',
                prevLabel: '<span style="color:green">Previous</span>',
                skipLabel: 'Exit',
                doneLabel: 'Got it!',
                tooltipPosition: 'auto'
            };
            ngIntroService.setOptions(otherUsersOptions);
            ngIntroService.start();
        }

        function setIntroNewUserswBalance() {
            var introOptionsNewUsers = {
                steps: [
                    {
                        element: "#chooseYourInv",
                        intro: "Choose your investment"
                    }
                ],
                showStepNumbers: false,
                exitOnOverlayClick: true,
                exitOnEsc: true,
                skipLabel: 'Exit',
                doneLabel: 'Go to selection',
                tooltipPosition: 'auto'
            }
            ngIntroService.setOptions(introOptionsNewUsers);

            ngIntroService.onComplete(function () {
                $location.path(constants.routes.portfolio.path).search({});
                $scope.$apply();
            });

            ngIntroService.start();
        }

        function setIntroOptionsForUsers(vintagesLength, balanceAmount, gmGainLoss) {
            if (vintagesLength === 0 && balanceAmount === 0) {
                setIntroNewUsers();

            } else if ((vintagesLength === 0 || vintagesLength === 1) && gmGainLoss < 0) {
                setIntroNewUserswBalance();

            } else if ( vintagesLength > 1 || balanceAmount === 0 ) {
                setIntroOtherUsers();
            }
        }

        $scope._init(function() {
            window.appInsights.trackEvent("INVESTMENT SUMMARY");
            loadSummaryData();
            loadAuthData();
        });
    }
})();