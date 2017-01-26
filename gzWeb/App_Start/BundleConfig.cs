using System;
using System.Configuration;
using System.Web.Optimization;

namespace gzWeb {
    public class BundleConfig {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles) {

            // Prefer cdn from local files
            bundles.UseCdn = true;

            #region Styles
            var bootstrapCdnPath = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css";
            bundles.Add(new StyleBundle("~/css/bootstrap", bootstrapCdnPath)
                .Include(
                    "~/Content/Styles/bootstrap/bootstrap.css", new CssRewriteUrlTransform()
                ).Include(
                    "~/Content/Styles/bootstrap/bootstrap-theme.css", new CssRewriteUrlTransform()
                ));

            var faCdnPath = "https://maxcdn.bootstrapcdn.com/font-awesome/4.6.3/css/font-awesome.min.css";
            bundles.Add(new StyleBundle("~/css/fa", faCdnPath).Include(
                "~/Content/Styles/font-awesome/font-awesome.css", new CssRewriteUrlTransform()
            ));

            bundles.Add(new StyleBundle("~/css/preloader").Include(
                "~/_app/common/preloader.css"
            ));
            bundles.Add(new StyleBundle("~/css/app").Include(
                "~/_app/common/basic.css"
                , "~/_app/common/header.css"
                , "~/_app/common/footer.css"
                , "~/_app/guest/guest.css"
                , "~/_app/account/auth.css"
                , "~/_app/investments/investments.css"
                , "~/_app/games/games.css"
                , "~/_app/games/carousel.css"
                , "~/_app/promotions/promotions.css"
                , "~/_app/accountManagement/accountManagement.css"
                , "~/_app/nsMessageService/nsMessages.css"
            ));
            #endregion

            #region Scripts
            bundles.Add(new ScriptBundle("~/js/asyncLoad").Include(
                "~/_app/common/asyncLoad.js"
            ));

            bundles.Add(new ScriptBundle("~/js/jquery").Include(
                "~/Scripts/jquery/jquery-{version}.js"
            ));

            //bundles.Add(new ScriptBundle("~/js/jqueryval").Include(
            //    "~/Scripts/jquery.validate/jquery.validate*"
            //));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/js/modernizr").Include(
            //    "~/Scripts/modernizr-*"
            //));

            bundles.Add(new ScriptBundle("~/js/bootstrap").Include(
                "~/Scripts/bootstrap/bootstrap.js"
                //, "~/Scripts/respond/respond.js"
            ));

            bundles.Add(new ScriptBundle("~/js/d3").Include(
                "~/Scripts/d3/d3.min.js"
            ));

            bundles.Add(new ScriptBundle("~/js/plugins").Include(
                "~/Scripts/moment/moment.min.js"
                , "~/Scripts/spin/spin.min.js"
            ));

            bundles.Add(new ScriptBundle("~/js/angular").Include(
                "~/Scripts/angular/angular.js"
                , "~/Scripts/angular-route/angular-route.min.js"
                , "~/Scripts/angular-resource/angular-resource.min.js"
                , "~/Scripts/angular-animate/angular-animate.min.js"
                , "~/Scripts/angular-sanitize/angular-sanitize.min.js"
                , "~/Scripts/angular-cookies/angular-cookies.min.js"
                , "~/Scripts/angular-touch/angular-touch.min.js"
            ));

            bundles.Add(new ScriptBundle("~/js/modules").Include(
                "~/Scripts/angular-ui/ui-bootstrap.min.js"
                , "~/Scripts/angular-ui/ui-bootstrap-tpls.min.js"
                , "~/Scripts/angular-filter/angular-filter.min.js"
                , "~/Scripts/angular-spinner/angular-spinner.min.js"
                , "~/Scripts/angular-moment/angular-moment.min.js"
                , "~/Scripts/angular-match-media/match-media.js"
                , "~/Scripts/angular-local-storage/angular-local-storage.min.js"
                , "~/Scripts/angular-fullscreen/angular-fullscreen.js"
                , "~/Scripts/angular-wamp/angular-wamp.min.js"
                , "~/Scripts/angular-autocomplete/ngAutocomplete.js"
                , "~/Scripts/angular-recaptcha/angular-recaptcha.min.js"
                , "~/Scripts/angular-iso-currency/isoCurrency.min.js"
                , "~/Scripts/angular-ui-datetime-picker/datetime-picker.min.js"
                , "~/Scripts/jsnlog/logToServer.js"
            ));

            var everyMatrixStageSetting = ConfigurationManager.AppSettings["everyMatrixStage"];
            var everyMatrixStage = !string.IsNullOrEmpty(everyMatrixStageSetting) && Convert.ToBoolean(everyMatrixStageSetting);

            bundles.Add(new ScriptBundle("~/js/app")
                                .Include("~/_app/common/app.js")
                                .Include(
            #region Common
                                        "~/_app/common/global.js"
                                        , "~/_app/common/config.js"
                                        , "~/_app/common/constants.js"
                                        , "~/_app/common/authCtrl.js"
                                        , "~/_app/common/headerCtrl.js"
                                        , "~/_app/common/footerCtrl.js"
            #endregion

            #region EveryMatrix
                                        , everyMatrixStage
                                            ? "~/_app/everymatrix/emConCfg-stage.js"
                                            : "~/_app/everymatrix/emConCfg.js"
                                        , "~/_app/everymatrix/emWamp.js"
                                        , "~/_app/everymatrix/emCasino.js"
                                        , "~/_app/everymatrix/emBanking.js"
                                        , "~/_app/everymatrix/emBankingWithdraw.js"
                                        , "~/_app/everymatrix/emResponsibleGaming.js"
            #endregion

            #region Controllers
                                        //, "~/Scripts/_app/controllers/authCtrl.js"
                                        //, "~/Scripts/_app/controllers/templates/headerCtrl.js"
                                        //, "~/Scripts/_app/controllers/templates/footerCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/homeCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/transparencyCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/aboutCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/contactCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/faqCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/helpCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/privacyCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/termsCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/playgroundCtrl.js"
                                        //, "~/Scripts/_app/controllers/investments/summaryCtrl.js"
                                        //, "~/Scripts/_app/controllers/investments/portfolioCtrl.js"
                                        //, "~/Scripts/_app/controllers/investments/performanceCtrl.js"
                                        //, "~/Scripts/_app/controllers/investments/activityCtrl.js"
                                        //, "~/Scripts/_app/controllers/investments/summaryVintagesCtrl.js"
                                        //, "~/Scripts/_app/controllers/games/gamesCtrl.js"
                                        //, "~/Scripts/_app/controllers/games/gameCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/registerCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/loginCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/investmentAccessErrorCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/forgotPasswordCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/resetPasswordCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/registerAccountCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/registerDetailsCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/registerPaymentMethodsCtrl.js"
                                        //, "~/Scripts/_app/controllers/account/registerDepositCtrl.js"
                                        //, "~/Scripts/_app/controllers/templates/registerDepositCreditCardCtrl.js"
                                        //, "~/Scripts/_app/controllers/templates/registerDepositMoneyMatrixCreditCardCtrl.js"
                                        //, "~/Scripts/_app/controllers/templates/registerDepositMoneyMatrixTrustlyCtrl.js"
            #endregion

            #region Guest
                                        , "~/_app/guest/homeCtrl.js"
                                        , "~/_app/guest/transparencyCtrl.js"
                                        , "~/_app/guest/aboutCtrl.js"
                                        , "~/_app/guest/contactCtrl.js"
                                        , "~/_app/guest/faqCtrl.js"
                                        , "~/_app/guest/helpCtrl.js"
                                        , "~/_app/guest/privacyGamesCtrl.js"
                                        , "~/_app/guest/privacyInvestmentCtrl.js"
                                        , "~/_app/guest/termsGamesCtrl.js"
                                        , "~/_app/guest/termsInvestmentCtrl.js"
                                        , "~/_app/guest/playgroundCtrl.js"
                                        , "~/_app/guest/responsibleGamblingCtrl.js"
            #endregion

            #region Account
                                        , "~/_app/account/loginCtrl.js"
                                        , "~/_app/account/investmentAccessErrorCtrl.js"
                                        , "~/_app/account/forgotPasswordCtrl.js"
                                        , "~/_app/account/resetPasswordCtrl.js"
                                        , "~/_app/account/registerAccountCtrl.js"
                                        , "~/_app/account/registerDetailsCtrl.js"
                                        , "~/_app/account/registerPaymentMethodsCtrl.js"
                                        , "~/_app/account/registerDepositCtrl.js"
                                        , "~/_app/account/registerDepositCreditCardCtrl.js"
                                        , "~/_app/account/registerDepositMoneyMatrixCreditCardCtrl.js"
                                        , "~/_app/account/registerDepositMoneyMatrixTrustlyCtrl.js"
			#endregion

            #region Account Management
                                        , "~/_app/accountManagement/accountManagementCtrl.js"
                                        , "~/_app/accountManagement/accountManagementService.js"

                                        , "~/_app/accountManagement/depositPaymentMethodsCtrl.js"
                                        , "~/_app/accountManagement/depositCtrl.js"
                                        , "~/_app/accountManagement/depositCreditCardCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixCreditCardCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixTrustlyCtrl.js"

                                        , "~/_app/accountManagement/withdrawPaymentMethodsCtrl.js"
                                        , "~/_app/accountManagement/withdrawCtrl.js"
                                        , "~/_app/accountManagement/withdrawCreditCardCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixCreditCardCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixTrustlyCtrl.js"

                                        , "~/_app/accountManagement/pendingWithdrawalsCtrl.js"
                                        , "~/_app/accountManagement/transactionHistoryCtrl.js"
                                        , "~/_app/accountManagement/bonusesCtrl.js"
                                        , "~/_app/accountManagement/responsibleGamingCtrl.js"
                                        , "~/_app/accountManagement/myProfileCtrl.js"
                                        , "~/_app/accountManagement/changePasswordCtrl.js"
            #endregion

            #region Investments
                                        , "~/_app/investments/summaryCtrl.js"
                                        , "~/_app/investments/portfolioCtrl.js"
                                        , "~/_app/investments/performanceCtrl.js"
                                        , "~/_app/investments/activityCtrl.js"
                                        , "~/_app/investments/summaryVintagesCtrl.js"
                                        , "~/_app/investments/gzPlansAllocationChart.js"
                                        , "~/_app/investments/gzPlanHoldingsChart.js"
                                        , "~/_app/investments/gzPerformanceGraph.js"
            #endregion

            #region Games
                                        , "~/_app/games/gamesCtrl.js"
                                        , "~/_app/games/gameCtrl.js"
                                        , "~/_app/games/gzFeaturedGame.js"
                                        , "~/_app/games/gzFeaturedGames.js"
                                        , "~/_app/games/gzCarousel.js"
            #endregion

            #region Promotions
                                        , "~/_app/promotions/promotionsCtrl.js"
                                        , "~/_app/promotions/promotionCtrl.js"
            #endregion

            #region Directives
                                        , "~/_app/directives/gzCheckBox.js"
                                        , "~/_app/directives/gzSelect.js"
                                        , "~/_app/directives/gzFieldOk.js"
                                        , "~/_app/directives/gzAuthAccess.js"
                                        , "~/_app/directives/gzThirdPartyIframe.js"
            #endregion

            #region Services
                                        , "~/_app/services/authInterceptor.js"
                                        , "~/_app/services/authService.js"
                                        , "~/_app/services/apiService.js"
                                        , "~/_app/services/helpersService.js"
                                        , "~/_app/services/chatService.js"
                                        , "~/_app/services/navService.js"
                                        , "~/_app/services/iovationService.js"
            #endregion

            #region nsMessageService
                                        , "~/_app/nsMessageService/nsMessage.js"
                                        , "~/_app/nsMessageService/nsMessages.js"
                                        , "~/_app/nsMessageService/nsPromptCtrl.js"
                                        , "~/_app/nsMessageService/nsConfirmCtrl.js"
                                        , "~/_app/nsMessageService/nsMessageService.js"
            #endregion

            #region Modules
                , "~/_app/modules/customDirectives.js"
                , "~/_app/modules/customFilters.js"
                //, "~/_app/modules/styleInjector.js"
            #endregion
                                ));

            #endregion
        }
    }
}
