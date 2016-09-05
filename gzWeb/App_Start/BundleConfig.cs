using System;
using System.Configuration;
using System.Web.Optimization;

namespace gzWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region Styles
            bundles.Add(new StyleBundle("~/css/bootstrap").Include(
                "~/Content/Styles/bootstrap/bootstrap.css", new CssRewriteUrlTransform()
            ).Include(
                "~/Content/Styles/bootstrap/bootstrap-theme.css", new CssRewriteUrlTransform()
            ));

            bundles.Add(new StyleBundle("~/css/fa").Include(
                "~/Content/Styles/font-awesome/font-awesome.min.css", new CssRewriteUrlTransform()
            ));

            bundles.Add(new StyleBundle("~/css/modules").Include(
            ));

            bundles.Add(new StyleBundle("~/css/app").Include(
                //"~/Content/Site.css"
                "~/Content/Styles/_app/basic.css"
                , "~/Content/Styles/_app/preloader.css"
                , "~/Content/Styles/_app/header.css"
                , "~/Content/Styles/_app/headerNew.css"
                , "~/Content/Styles/_app/footer.css"
                , "~/Content/Styles/_app/auth.css"
                , "~/Content/Styles/_app/investments.css"
                
                , "~/_app/games/games.css"
                , "~/_app/games/carousel.css"
                , "~/_app/guest/guest.css"
                , "~/_app/accountManagement/accountManagement.css"
                , "~/Scripts/_app/services/nsMessageService/nsMessages.css"
            ));
            #endregion

            #region Scripts
            bundles.Add(new ScriptBundle("~/js/global").Include(
                "~/Scripts/_app/global.js"
            ));

            bundles.Add(new ScriptBundle("~/js/jquery").Include(
                "~/Scripts/jquery/jquery-{version}.js"
            ));

            bundles.Add(new ScriptBundle("~/js/jqueryval").Include(
                "~/Scripts/jquery.validate/jquery.validate*"
            ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/js/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            bundles.Add(new ScriptBundle("~/js/bootstrap").Include(
                "~/Scripts/bootstrap/bootstrap.js"
                , "~/Scripts/respond/respond.js"
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
                , "~/Scripts/angular-wamp/angular-wamp.js" // TODO: "~/Scripts/angular-wamp/angular-wamp.min.js"
                , "~/Scripts/angular-autocomplete/ngAutocomplete.js"
                , "~/Scripts/angular-recaptcha/angular-recaptcha.min.js"
                , "~/Scripts/angular-iso-currency/isoCurrency.min.js"
                , "~/Scripts/_modules/customDirectives.js"
                , "~/Scripts/_modules/customFilters.js"
                , "~/Scripts/jsnlog/logToServer.js"
                //, "~/Scripts/_modules/styleInjector.js"
            ));

            var everyMatrixStageSetting = ConfigurationManager.AppSettings["everyMatrixStage"];
            var everyMatrixStage = !string.IsNullOrEmpty(everyMatrixStageSetting) && Convert.ToBoolean(everyMatrixStageSetting);

            bundles.Add(new ScriptBundle("~/js/app")
                                .Include("~/Scripts/_app/app.js")
                                .Include("~/Scripts/_app/configuration/config.js"
                                        , "~/Scripts/_app/constants/constants.js"
                                        , everyMatrixStage
                                                  ? "~/Scripts/_app/constants/emConCfg-stage.js"
                                                  : "~/Scripts/_app/constants/emConCfg.js"

                                        #region Controllers
                                        , "~/Scripts/_app/controllers/authCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/headerCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/headerNewCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/footerCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/registerDepositCreditCardCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/registerDepositMoneyMatrixCreditCardCtrl.js"
                                        , "~/Scripts/_app/controllers/templates/registerDepositMoneyMatrixTrustlyCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/homeCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/transparencyCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/aboutCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/contactCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/faqCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/helpCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/privacyCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/termsCtrl.js"
                                        //, "~/Scripts/_app/controllers/guest/playgroundCtrl.js"
                                        , "~/Scripts/_app/controllers/investments/summaryCtrl.js"
                                        , "~/Scripts/_app/controllers/investments/portfolioCtrl.js"
                                        , "~/Scripts/_app/controllers/investments/performanceCtrl.js"
                                        , "~/Scripts/_app/controllers/investments/activityCtrl.js"
                                        , "~/Scripts/_app/controllers/investments/summaryVintagesCtrl.js"
                                        //, "~/Scripts/_app/controllers/games/gamesCtrl.js"
                                        //, "~/Scripts/_app/controllers/games/gameCtrl.js"
                                        , "~/Scripts/_app/controllers/account/registerCtrl.js"
                                        , "~/Scripts/_app/controllers/account/loginCtrl.js"
                                        , "~/Scripts/_app/controllers/account/investmentAccessErrorCtrl.js"
                                        , "~/Scripts/_app/controllers/account/forgotPasswordCtrl.js"
                                        , "~/Scripts/_app/controllers/account/resetPasswordCtrl.js"
                                        , "~/Scripts/_app/controllers/account/registerAccountCtrl.js"
                                        , "~/Scripts/_app/controllers/account/registerDetailsCtrl.js"
                                        , "~/Scripts/_app/controllers/account/registerPaymentMethodsCtrl.js"
                                        , "~/Scripts/_app/controllers/account/registerDepositCtrl.js"
                                        #endregion

                                        #region Guest
                                        , "~/_app/guest/homeCtrl.js"
                                        , "~/_app/guest/transparencyCtrl.js"
                                        , "~/_app/guest/aboutCtrl.js"
                                        , "~/_app/guest/contactCtrl.js"
                                        , "~/_app/guest/faqCtrl.js"
                                        , "~/_app/guest/helpCtrl.js"
                                        , "~/_app/guest/privacyCtrl.js"
                                        , "~/_app/guest/termsCtrl.js"
                                        , "~/_app/guest/playgroundCtrl.js"
                                        #endregion

                                        #region Games
                                        , "~/_app/games/gamesCtrl.js"
                                        , "~/_app/games/gameCtrl.js"
                                        , "~/_app/games/gzFeaturedGame.js"
                                        , "~/_app/games/gzFeaturedGames.js"
                                        , "~/_app/games/gzCarousel.js"
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

                                        #region Directives
                                        , "~/Scripts/_app/directives/gzPlansAllocationChart.js"
                                        , "~/Scripts/_app/directives/gzPlanHoldingsChart.js"
                                        , "~/Scripts/_app/directives/gzPerformanceGraph.js"
                                        , "~/Scripts/_app/directives/gzCheckBox.js"
                                        , "~/Scripts/_app/directives/gzSelect.js"
                                        , "~/Scripts/_app/directives/gzFieldOk.js"
                                        , "~/Scripts/_app/directives/gzAuthAccess.js"
                                        , "~/Scripts/_app/directives/gzThirdPartyIframe.js"
                                        #endregion

                                        #region Services
                                        , "~/Scripts/_app/services/emWamp.js"
                                        , "~/Scripts/_app/services/emCasino.js"
                                        , "~/Scripts/_app/services/emBanking.js"
                                        , "~/Scripts/_app/services/emBankingWithdraw.js"
                                        , "~/Scripts/_app/services/emResponsibleGaming.js"
                                        , "~/Scripts/_app/services/authInterceptor.js"
                                        , "~/Scripts/_app/services/authService.js"
                                        , "~/Scripts/_app/services/apiService.js"
                                        , "~/Scripts/_app/services/helpersService.js"
                                        , "~/Scripts/_app/services/chatService.js"
                                        , "~/Scripts/_app/services/iovationService.js"
                                        #endregion

                                        #region nsMessageService
                                        , "~/Scripts/_app/services/nsMessageService/nsMessage.js"
                                        , "~/Scripts/_app/services/nsMessageService/nsMessages.js"
                                        , "~/Scripts/_app/services/nsMessageService/nsPromptCtrl.js"
                                        , "~/Scripts/_app/services/nsMessageService/nsConfirmCtrl.js"
                                        , "~/Scripts/_app/services/nsMessageService/nsMessageService.js"
                                        #endregion
                                ));

            #endregion
        }
    }
}
