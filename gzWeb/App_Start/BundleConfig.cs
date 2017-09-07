﻿using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Optimization;

namespace gzWeb
{
    public class BundleConfig
    {
        private static readonly CssRewriteUrlTransform CssImageRelPathFixer = new CssRewriteUrlTransform();

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            const string gzStaticCdnUrl = "https://gz.azureedge.net{0}?v={1}";

#if DEBUG
            // Debug cdn paths
            //BundleTable.EnableOptimizations = true;
            bundles.UseCdn = false; // true;
#else
            // Possibility to Override Cdn use in www.greenzorro.com
            bundles.UseCdn = Boolean.Parse(ConfigurationManager.AppSettings["UseCDN"]);;
#endif

            #region Styles
            bundles.Add(new StyleBundle("~/css/preloader").Include(
                "~/_app/common/preloader.css"
            ));

            CreateCssBundleCdn(
                bundles,
                "~/css/bootstrap",
                "~/Content/Styles/bootstrap/bootstrap.css",
                "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css");

            CreateCssBundleCdn(
                bundles,
                "~/css/bootstrap-theme",
                "~/Content/Styles/bootstrap/bootstrap-theme.min.css",
                "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css");

            CreateCssBundleCdn(
                bundles,
                "~/css/fa",
                "~/Content/Styles/font-awesome/font-awesome.min.css",
                "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css");

            CreateCssBundleCdn(
                bundles,
                "~/css/app",
                new string[] {
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
                },
                gzStaticCdnUrl);

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
                , "~/Scripts/autobahn/autobahn.min.js"
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
                , "~/Scripts/angular-wamp/angular-wamp.js"
                , "~/Scripts/angular-autocomplete/ngAutocomplete.js"
                , "~/Scripts/angular-recaptcha/angular-recaptcha.min.js"
                , "~/Scripts/angular-iso-currency/isoCurrency.min.js"
                , "~/Scripts/angular-ui-datetime-picker/datetime-picker.min.js"
                , "~/Scripts/jsnlog/jsnlog.min.js"
                , "~/Scripts/jsnlog/logToServer.js"
                , "~/Scripts/angular-appinsights/angular-appinsights.js"

                , "~/_app/modules/customDirectives.js"
                , "~/_app/modules/customFilters.js"
            ));

            var everyMatrixStageSetting = ConfigurationManager.AppSettings["everyMatrixStage"];
            var everyMatrixStage = !string.IsNullOrEmpty(everyMatrixStageSetting) && Convert.ToBoolean(everyMatrixStageSetting);

            var appInsightsEnvironmentKeySetting = ConfigurationManager.AppSettings["appInsightsEnvKey"];
            var appInsightsEnvironmentKey = string.IsNullOrEmpty(appInsightsEnvironmentKeySetting)
                ? "Dev"
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(appInsightsEnvironmentKeySetting);
            //: (appInsightsEnvironmentKeySetting.First().ToString().ToUpperInvariant() + appInsightsEnvironmentKeySetting.Substring(1).ToLowerInvariant());
            var appInsightsCfg = $"~/_app/common/appInsights{appInsightsEnvironmentKey}Cfg.js";

            bundles.Add(new ScriptBundle("~/js/app")
                                .Include("~/_app/common/app.js")
                                .Include(
            #region Common
                                        "~/_app/common/global.js"
                                        , "~/_app/common/config.js"
                                        , "~/_app/common/constants.js"
                                        , appInsightsCfg
                                        , "~/_app/common/authCtrl.js"
                                        , "~/_app/common/headerCtrl.js"
                                        , "~/_app/common/footerCtrl.js"
                                        , "~/_app/common/footerMenu.js"
                                        , "~/_app/common/apiErrorCtrl.js"
            #endregion

            #region EveryMatrix
                                        , everyMatrixStage ? "~/_app/everymatrix/emConCfg-stage.js" : "~/_app/everymatrix/emConCfg.js"
                                        , "~/_app/everymatrix/emWamp.js"
                                        , "~/_app/everymatrix/emCasino.js"
                                        , "~/_app/everymatrix/emBanking.js"
                                        , "~/_app/everymatrix/emBankingWithdraw.js"
                                        , "~/_app/everymatrix/emResponsibleGaming.js"
                                        , "~/_app/everymatrix/challengeCtrl.js"
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
                                        , "~/_app/account/registerDepositMoneyMatrixSkrillCtrl.js"
                                        , "~/_app/account/registerDepositMoneyMatrixSkrill1TapCtrl.js"
                                        , "~/_app/account/registerDepositMoneyMatrixEnterCashCtrl.js"
                                        , "~/_app/account/receiptCtrl.js"
            #endregion

            #region Account Management
                                        , "~/_app/accountManagement/accountManagementCtrl.js"
                                        , "~/_app/accountManagement/accountManagementService.js"

                                        , "~/_app/accountManagement/depositPaymentMethodsCtrl.js"
                                        , "~/_app/accountManagement/depositCtrl.js"
                                        , "~/_app/accountManagement/depositCreditCardCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixCreditCardCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixTrustlyCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixSkrillCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixSkrill1TapCtrl.js"
                                        , "~/_app/accountManagement/depositMoneyMatrixEnterCashCtrl.js"
                                        //, "~/_app/accountManagement/depositMoneyMatrixNetellerCtrl.js"
                                        //, "~/_app/accountManagement/depositMoneyMatrixPaySafeCardCtrl.js"
                                        //, "~/_app/accountManagement/depositMoneyMatrixEcoPayzCtrl.js"

                                        , "~/_app/accountManagement/withdrawPaymentMethodsCtrl.js"
                                        , "~/_app/accountManagement/withdrawCtrl.js"
                                        , "~/_app/accountManagement/withdrawFieldsCtrl.js"
                                        //, "~/_app/accountManagement/withdrawCreditCardCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixCommonCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixCreditCardCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixTrustlyCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixSkrillCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixSkrill1TapCtrl.js"
                                        , "~/_app/accountManagement/withdrawMoneyMatrixEnterCashCtrl.js"
                                        //, "~/_app/accountManagement/withdrawMoneyMatrixNetellerCtrl.js"
                                        //, "~/_app/accountManagement/withdrawMoneyMatrixPaySafeCardCtrl.js"
                                        //, "~/_app/accountManagement/withdrawMoneyMatrixEcoPayzCtrl.js"

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
                                        , "~/_app/games/carouselVideoCtrl.js"
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
                                        , "~/_app/services/modalsService.js"
            #endregion

            #region nsMessageService
                                        , "~/_app/nsMessageService/nsMessage.js"
                                        , "~/_app/nsMessageService/nsMessages.js"
                                        , "~/_app/nsMessageService/nsPromptCtrl.js"
                                        , "~/_app/nsMessageService/nsConfirmCtrl.js"
                                        , "~/_app/nsMessageService/nsMessageService.js"
            #endregion

            #region Bootstrap
                                        , "~/_app/common/bootstrap.js"
            #endregion
                                ));

            #endregion
        }

        private static void CreateCssBundleCdn(
            BundleCollection bundles, 
            string bundleKeyRelPath,
            string cssLocalPath,
            string cssCdnPath
            )
        {
            var cssBundle = new StyleBundle(bundleKeyRelPath)
                .Include(
                    cssLocalPath, CssImageRelPathFixer
                );
            bundles.Add(cssBundle);
            /**
             * Said that creating the hash
             * before setting the CdnPath
             * results in different value and throwing off browser caching.
             * Observation has indicated this bug is fixed in system.web.optiomization.
             */
            var cssCdnPathssBundleHash = GetBundleHash(bundles, bundleKeyRelPath);
            cssBundle.CdnPath = cssCdnPath + "?v=" + cssCdnPathssBundleHash;
        }

        private static void CreateCssBundleCdn(
            BundleCollection bundles,
            string bundleKeyRelPath,
            string[] cssLocalPaths,
            string cssCdnPath
        )
        {
            var cssBundle = new StyleBundle(bundleKeyRelPath)
                .Include(
                    cssLocalPaths
                );
            bundles.Add(cssBundle);
            var cssBundleHash = GetBundleHash(bundles, bundleKeyRelPath);
            var bundleKeyCleanRelPath = bundleKeyRelPath.Substring(1, bundleKeyRelPath.Length - 1);
            cssBundle.CdnPath = string.Format(cssCdnPath, bundleKeyCleanRelPath, cssBundleHash);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/35543576/mvc-5-bundling-and-azure-cdn-query-string?rq=1
        /// </summary>
        /// <param name="bundles"></param>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        private static string GetBundleHash(BundleCollection bundles, string bundlePath)
        {

            //Need the context to generate response
            var bundleContext = new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, bundlePath);

            //Bundle class has the method we need to get a BundleResponse
            Bundle bundle = BundleTable.Bundles.GetBundleFor(bundlePath);
            var bundleResponse = bundle.GenerateBundleResponse(bundleContext);

            //BundleResponse has the method we need to call, but its marked as
            //internal and therefor is not available for public consumption.
            //To bypass this, reflect on it and manually invoke the method
            var bundleReflection = bundleResponse.GetType();

            var method = bundleReflection.GetMethod("GetContentHashCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //contentHash is whats appended to your url (url?###-###...)
            var contentHash = method.Invoke(bundleResponse, null);
            return contentHash.ToString();
        }

    }
}
