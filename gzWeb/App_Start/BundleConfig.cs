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
                "~/Content/Styles/ng-tags-input/ng-tags-input.min.css"
            ));

            //bundles.Add(new StyleBundle("~/css/app_old").Include(
            //    //"~/Content/Site.css"
            //    "~/Content/Styles/_app/template/basic.css"
            //    , "~/Content/Styles/_app/template/acorns.css"
            //    , "~/Content/Styles/_app/template/donut.css"
            //));
            bundles.Add(new StyleBundle("~/css/app").Include(
                //"~/Content/Site.css"
                "~/Content/Styles/_app/basic.css"
                , "~/Content/Styles/_app/preloader.css"
                , "~/Content/Styles/_app/header.css"
                , "~/Content/Styles/_app/headerNew.css"
                , "~/Content/Styles/_app/footer.css"
                , "~/Content/Styles/_app/guest.css"
                , "~/Content/Styles/_app/auth.css"
                , "~/Content/Styles/_app/investments.css"
                , "~/Content/Styles/_app/games.css"
                , "~/Scripts/_app/services/nsMessageService/nsMessages.css"
                //, "~/Content/Styles/_app/exceptionaire/exceptionaire-meterialize.css"
                //, "~/Content/Styles/_app/exceptionaire/exceptionaire-style.css"
                //, "~/Content/Styles/_app/exceptionaire/exceptionaire-media.css"
            ));
            #endregion
            
            #region Scripts
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
                , "~/Scripts/d3-tip/d3-tip.js"
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
            ));

            bundles.Add(new ScriptBundle("~/js/modules").Include(
                "~/Scripts/angular-ui/ui-bootstrap.min.js"
                , "~/Scripts/angular-ui/ui-bootstrap-tpls.min.js"
                , "~/Scripts/angular-filter/angular-filter.min.js"
                , "~/Scripts/angular-spinner/angular-spinner.min.js"
                , "~/Scripts/angular-moment/angular-moment.min.js"
                , "~/Scripts/angular-match-media/match-media.js"
                , "~/Scripts/angular-local-storage/angular-local-storage.min.js"
                , "~/Scripts/angular-count-to/angular-count-to.min.js"
                , "~/Scripts/angular-fullscreen/angular-fullscreen.js"
                , "~/Scripts/angular-wamp/angular-wamp.js" // TODO: "~/Scripts/angular-wamp/angular-wamp.min.js"
                , "~/Scripts/angular-autocomplete/ngAutocomplete.js"
                , "~/Scripts/angular-recaptcha/angular-recaptcha.min.js"
                , "~/Scripts/angular-iso-currency/isoCurrency.min.js"
                , "~/Scripts/ng-tags-input/ng-tags-input.min.js"
                , "~/Scripts/_modules/customDirectives.js"
                , "~/Scripts/_modules/customFilters.js"
                //, "~/Scripts/_modules/styleInjector.js"
            ));

            bundles.Add(new ScriptBundle("~/js/app").Include(
                "~/Scripts/_app/app.js"
            ).Include(
                "~/Scripts/_app/configuration/config.js"
                , "~/Scripts/_app/constants/constants.js"

                , "~/Scripts/_app/controllers/authCtrl.js"
                , "~/Scripts/_app/controllers/templates/headerCtrl.js"
                , "~/Scripts/_app/controllers/templates/footerCtrl.js"
                , "~/Scripts/_app/controllers/templates/headerNewCtrl.js"
                , "~/Scripts/_app/controllers/templates/footerNewCtrl.js"
                , "~/Scripts/_app/controllers/templates/depositCreditCardCtrl.js"
                , "~/Scripts/_app/controllers/templates/depositTrustlyCtrl.js"
                , "~/Scripts/_app/controllers/guest/homeCtrl.js"
                , "~/Scripts/_app/controllers/guest/transparencyCtrl.js"
                , "~/Scripts/_app/controllers/guest/aboutCtrl.js"
                , "~/Scripts/_app/controllers/guest/contactCtrl.js"
                , "~/Scripts/_app/controllers/guest/faqCtrl.js"
                , "~/Scripts/_app/controllers/guest/playgroundCtrl.js"
                , "~/Scripts/_app/controllers/investments/summaryCtrl.js"
                , "~/Scripts/_app/controllers/investments/portfolioCtrl.js"
                , "~/Scripts/_app/controllers/investments/performanceCtrl.js"
                , "~/Scripts/_app/controllers/investments/activityCtrl.js"
                , "~/Scripts/_app/controllers/investments/summaryVintagesCtrl.js"
                , "~/Scripts/_app/controllers/games/gamesCtrl.js"
                , "~/Scripts/_app/controllers/games/gameCtrl.js"
                , "~/Scripts/_app/controllers/account/registerCtrl.js"
                , "~/Scripts/_app/controllers/account/loginCtrl.js"
                , "~/Scripts/_app/controllers/account/forgotPasswordCtrl.js"
                , "~/Scripts/_app/controllers/account/resetPasswordCtrl.js"
                , "~/Scripts/_app/controllers/account/changePasswordCtrl.js"
                , "~/Scripts/_app/controllers/account/registerAccountCtrl.js"
                , "~/Scripts/_app/controllers/account/registerDetailsCtrl.js"
                , "~/Scripts/_app/controllers/account/registerPaymentMethodsCtrl.js"
                , "~/Scripts/_app/controllers/account/registerDepositCtrl.js"
                //ACCOUNT MANAGEMENT
                , "~/Scripts/_app/controllers/account/depositCtrl.js"
                , "~/Scripts/_app/controllers/account/withdrawCtrl.js"
                , "~/Scripts/_app/controllers/account/pendingWithdrawalsCtrl.js"
                , "~/Scripts/_app/controllers/account/transactionHistoryCtrl.js"
                , "~/Scripts/_app/controllers/account/bonusesCtrl.js"
                , "~/Scripts/_app/controllers/account/myProfileCtrl.js"


                , "~/Scripts/_app/directives/gzPortfolioChart.js"
                , "~/Scripts/_app/directives/gzPerformanceGraph.js"
                , "~/Scripts/_app/directives/gzCheckBox.js"
                , "~/Scripts/_app/directives/gzSelect.js"
                , "~/Scripts/_app/directives/gzFieldOk.js"
                , "~/Scripts/_app/directives/gzAuthAccess.js"
                , "~/Scripts/_app/directives/gzFeaturedGame.js"
                , "~/Scripts/_app/directives/gzFeaturedGames.js"

                , "~/Scripts/_app/services/emWamp.js"
                , "~/Scripts/_app/services/emCasino.js"
                , "~/Scripts/_app/services/emBanking.js"
                , "~/Scripts/_app/services/authInterceptor.js"
                , "~/Scripts/_app/services/authService.js"
                , "~/Scripts/_app/services/apiService.js"
                , "~/Scripts/_app/services/helpersService.js"
                , "~/Scripts/_app/services/chatService.js"

                , "~/Scripts/_app/services/nsMessageService/nsMessage.js"
                , "~/Scripts/_app/services/nsMessageService/nsMessages.js"
                , "~/Scripts/_app/services/nsMessageService/nsPromptCtrl.js"
                , "~/Scripts/_app/services/nsMessageService/nsConfirmCtrl.js"
                , "~/Scripts/_app/services/nsMessageService/nsMessageService.js"

                //, "~/Scripts/_app/exceptionaire/exceptionaire-materialize.min.js"
            ));
            #endregion
        }
    }
}
