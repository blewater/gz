using gzDAL.Models;
using gzDAL.Repos;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;

namespace gzDAL.Conf {

    /// <summary>
    /// Unit Testing Seed class
    /// 
    ///         Conf.TestSeeder.GenData();
    /// 
    /// If need be here's
    /// how to Truncate tables
    /// http://stackoverflow.com/questions/24209220/ef6-code-first-drop-tables-not-entire-database-when-model-changes
    /// </summary>
    public class TestSeeder {

        public static string GenData() {

            //Assume success
            string retExceptionVal = null;

            // Tracing log location + name i.e. C:\Users\Mario\AppData\Local\Temp\sqlLogFile.log
            var tempSQLLogPath = Path.GetTempPath() + "sqlLogFile.log";

            Database.SetInitializer<TestDbContext>(null);

            //http://stackoverflow.com/questions/815586/entity-framework-using-transactions-or-savechangesfalse-and-acceptallchanges
            using (var sqlLogFile = new StreamWriter(tempSQLLogPath)) {
                using (TestDbContext context = new TestDbContext()) {
                    context.Database.Log = sqlLogFile.Write;
                    // Decided against enclosing in a all encompassing transactions because of frequent failures due to the schema volatility and the need to get immediate feedback on the failure
                    try {

                        AddUpdData(context);

                    } catch (Exception ex) {
                        var exception = retExceptionVal = ex.Message;
                        throw;
                    }
                }
            }
            return retExceptionVal;
        }

        /// <summary>
        /// Calling AddUpdData on all domain objects.
        /// </summary>
        /// <param name="context"></param>
        private static void AddUpdData(TestDbContext context) {

            // GzConfigurations
            CreateUpdConfiguationRow(context);

            // Customers
            var manager = new ApplicationUserManager(new CustomUserStore(context),
                                                     new DataProtectionProviderFactory(() => null));
            int custId = SaveDbCreateUser(manager, context);

            // Currencies
            CreateUpdCurrenciesList(context);
            context.SaveChanges();

            // Funds
            CreateUpdFunds(context);
            context.SaveChanges();

            // FundPrices
            CreateUpdFundsPrices(context);
            context.SaveChanges();

            // Portfolios
            CreateUpdPortfolios(context, custId);
            context.SaveChanges();

            // Link now a portfolio for this customer
            var custPortfolioRepo = new CustPortfolioRepo(context);
            custPortfolioRepo.SaveDbCustMonthsPortfolioMix(custId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));

            // Portfolios - Funds association table
            CreateUpdPortFunds(context);
            context.SaveChanges();

            // gzTransactionTypes
            CreateUpdTranxType(context);
            context.SaveChanges();

            // gzTransactions
            CreateUpdGzTransaction(context, custId);
            context.SaveChanges();

            // Balances
            CalcMonthlyBalances(context, custId);
            context.SaveChanges();
        }

        /// <summary>
        /// Insert or Update the gzConfiguration Constants in a single row
        /// </summary>
        /// <param name="context"></param>
        private static void CreateUpdConfiguationRow(TestDbContext context) {

            var dbConf = new GzConfiguration();

            context.GzConfigurations.AddOrUpdate(
                c => c.Id,
                new GzConfiguration {
                    // Essentially write out the defaults in a single row
                    Id = 1,
                    COMMISSION_PCNT = dbConf.COMMISSION_PCNT,
                    CREDIT_LOSS_PCNT = dbConf.CREDIT_LOSS_PCNT,
                    FUND_FEE_PCNT = dbConf.FUND_FEE_PCNT,
                    LOCK_IN_NUM_DAYS = dbConf.LOCK_IN_NUM_DAYS,
                    FIRST_PORTFOLIO_RISK_VAL = RiskToleranceEnum.Medium,
                    CONSERVATIVE_RISK_ROI = 3f,
                    MEDIUM_RISK_ROI = 6f,
                    AGGRESSIVE_RISK_ROI = 10f
                }
            );

            context.SaveChanges();
        }

        /// <summary>
        /// Add or Update existing test users
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        private static int SaveDbCreateUser(ApplicationUserManager manager, TestDbContext context) {

            var newUser = new ApplicationUser() {
                UserName = "testuser",
                Email = "testuser@gz.com",
                EmailConfirmed = true,
                FirstName = "test",
                LastName = "user",
                Birthday = new DateTime(1975, 10, 13),
                Currency = "SEK",
                PasswordHash = manager.PasswordHasher.HashPassword("gz2016!@")
            };

            context.Users.AddOrUpdate(c => new { c.Email }, newUser);
            context.SaveChanges();

            var custId = context.Users.Where(u => u.Email == newUser.Email).Select(u => u.Id).Single();
            return custId;
        }

        /// <summary>
        /// 
        /// Create Everymatrix user if not existing in the current database.
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="manager"></param>
        /// <param name="everyMatrixEmail"></param>
        /// <param name="everyMatrixUsername"></param>
        /// <param name="everyMatrixPwd"></param>
        /// <param name="everyMatrixFirstName"></param>
        /// <param name="everyMatrixLastName"></param>
        /// <param name="doB"></param>
        /// <returns></returns>
        private static int SaveDbCreateStageEverymatrixUser(
            TestDbContext db,
            ApplicationUserManager manager,
            string everyMatrixEmail,
            string everyMatrixUsername,
            string everyMatrixPwd,
            string everyMatrixFirstName,
            string everyMatrixLastName,
            DateTime doB) {

            db.Users.AddOrUpdate(
                c => new { c.Email },
                new ApplicationUser() {
                    UserName = everyMatrixUsername,
                    Email = everyMatrixEmail,
                    EmailConfirmed = true,
                    FirstName = everyMatrixFirstName,
                    LastName = everyMatrixLastName,
                    Birthday = doB,
                    Currency = "SEK"
                });

            var custId = manager.FindByEmail(everyMatrixEmail).Id;
            return custId;
        }

        /// <summary>
        /// Calculate the monthly balances for a customer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="custId"></param>
        private static void CalcMonthlyBalances(TestDbContext context, int custId) {

            var custPortfolioRepo = new CustPortfolioRepo(context);
            new InvBalanceRepo(
                context, 
                new CustFundShareRepo(
                    context,
                    custPortfolioRepo), 
                new GzTransactionRepo(context),
                custPortfolioRepo)
                .SaveDbCustomerAllMonthlyBalances(custId);
        }

        private static void CreateUpdCurrenciesList(TestDbContext context) {

            var usd = "USD";

            context.CurrenciesListX.AddOrUpdate(
                c => new { c.From, c.To },
                new CurrencyListX {
                    From = "AUD", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "CAD", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "CHF", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "DKK", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "EUR", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "GBP", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "NOK", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = "SEK", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "AUD", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "CAD", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "CHF", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "DKK", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "EUR", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "GBP", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "NOK", UpdatedOnUTC = DateTime.UtcNow
                },
                new CurrencyListX {
                    From = usd, To = "SEK", UpdatedOnUTC = DateTime.UtcNow
                }
                );
        }

        private static void CreateUpdFunds(TestDbContext context) {
            context.Funds.AddOrUpdate(
                f => f.HoldingName,
                new Fund {
                    HoldingName = "iShares National Muni Bond", Symbol = "MUB", YearToDate = 4.63f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                },
                new Fund {
                    HoldingName = "Vanguard Short-Term Federal", Symbol = "VSGBX", YearToDate = 2.02f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                },
                new Fund {
                    HoldingName = "Vanguard VTI", Symbol = "VTI", YearToDate = 6.15f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                },
                new Fund {
                    HoldingName = "Vanguard Total Bond Market Index", Symbol = "VBMFX", YearToDate = 5.88f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                },
                new Fund {
                    HoldingName = "Vanguard Strategic Small-Cap Equity", Symbol = "VSTCX", YearToDate = 8.30f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                },
                new Fund {
                    HoldingName = "Victory CEMP US 500 Enhanced Vol Wtd", Symbol = "CFA", YearToDate = 11.21f, UpdatedOnUTC = new DateTime(2017, 1, 11, 9, 34, 0)
                }
                );
        }

        private static void CreateUpdFundsPrices(TestDbContext context) {
            context.FundPrices.AddOrUpdate(
                f => new { f.FundId, f.YearMonthDay },

                // End of 2014
                new FundPrice {
                    ClosingPrice = 110.34F,
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 106.00F,
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 54.24F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSGBX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 37.88F,
                    FundId = context.Funds.Where(f => f.Symbol == "VBMFX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 40.02F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSTCX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 79.16F,
                    FundId = context.Funds.Where(f => f.Symbol == "CFA").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },

                // June 2015
                new FundPrice {
                    ClosingPrice = 108.44F,
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 109.58F,
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 54.44F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSGBX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 41.25F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSTCX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 42.31F,
                    FundId = context.Funds.Where(f => f.Symbol == "CFA").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 78.19F,
                    FundId = context.Funds.Where(f => f.Symbol == "VBMFX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                }
                //, later with the rest
                //new FundPrice {
                //    ClosingPrice = 108.79F,
                //    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                //    YearMonthDay = "20150721",
                //    UpdatedOnUTC = new DateTime(2015, 7, 21, 23, 50, 0)
                //}
                );
        }

        private static void CreateUpdPortfolios(TestDbContext context, int custId) {

            context.Portfolios.AddOrUpdate(
                p => p.RiskTolerance,
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Low,
                    IsActive = true,
                    Color = "#B4DCC4",
                    Title = "Conservative"
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Low_Medium,
                    IsActive = false,
                    Color = "#FF0000",
                    Title = "Low Medium"
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Medium,
                    IsActive = true,
                    Color = "#64BF89",
                    Title = "Medium"
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Medium_High,
                    IsActive = false,
                    Color = "#00FF00",
                    Title = "Medium High"
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.High,
                    IsActive = true,
                    Color = "#227B46",
                    Title = "Aggressive"
                });

        }

        private static void CreateUpdPortFunds(TestDbContext context) {
            context.PortFunds.AddOrUpdate(
                p => new { p.PortfolioId, p.FundId },
                // LOW
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VSGBX").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                // MEDIUM
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VBMFX").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                // HIGH
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VSTCX").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "CFA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 50f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdGzTransaction(TestDbContext context, int custId) {

            var trxRepo = new GzTransactionRepo(context);

            // Use new API
            trxRepo.SaveDbPlayingLoss(custId, 9853, new DateTime(2015, 3, 31, 23, 46, 01));
            trxRepo.SaveDbPlayingLoss(custId, 2013, new DateTime(2015, 4, 29, 23, 56, 12, 42));

            // Old implementation before repo
            context.GzTrxs.AddOrUpdate(
                t => new { t.CustomerId, t.CreatedOnUtc },
                // May
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201505",
                    CreatedOnUtc = new DateTime(2015, 5, 31, 23, 47, 12),
                    Amount = new decimal(784.22),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // June
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201506",
                    CreatedOnUtc = new DateTime(2015, 6, 30, 23, 47, 32),
                    Amount = new decimal(1192.11),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // August Skip July he won
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201508",
                    CreatedOnUtc = new DateTime(2015, 8, 31, 23, 47, 46),
                    Amount = new decimal(292.12),
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Sept
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201509",
                    CreatedOnUtc = new DateTime(2015, 9, 30, 23, 47, 46),
                    Amount = new decimal(1471.5),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Oct
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201510",
                    CreatedOnUtc = new DateTime(2015, 10, 31, 23, 47, 46),
                    Amount = new decimal(916.11),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Dec Skip Nov either won or did not play
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201512",
                    CreatedOnUtc = new DateTime(2015, 12, 31, 23, 47, 12),
                    Amount = new decimal(1677.02),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdTranxType(TestDbContext context) {
            context.GmTrxTypes.AddOrUpdate(
                t => t.Code,
                new GmTrxType {
                    Code = GmTransactionTypeEnum.Deposit,
                    Description = "Customer deposit to their casino account. Informational purpose only."
                }
                ,
                new GmTrxType {
                    Code = GmTransactionTypeEnum.CasinoWithdrawal,
                    Description = "Player cash withdrawal from their casino account. Informational purpose only"
                }
                ,
                new GmTrxType {
                    Code = GmTransactionTypeEnum.PlayingLoss,
                    Description =
                        "Customer Casino loss. We credit a percentage i.e. 50% from this amount; see CreditedPlayingLoss."
                }
                );
            context.GzTrxTypes.AddOrUpdate(
                t => t.Code,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.InvWithdrawal,
                    Description = "Reserved for future use. Customer withdrawal from their greenzorro account to their banking account. Generate fees transactions: FundFee, Commission (4%)."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.TransferToGaming,
                    Description = "Liquidate a single month's customer's investment(vintage) and transfer cash to their casino account. greenzorro will follow up operationally with a greenzorro debit of banking cash to credit the casino's customer's banking account."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.FullCustomerFundsLiquidation,
                    Description = "Liquidate all customer\'s funds to cash. Typical Transaction when closing a customer\'s account."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.CreditedPlayingLoss,
                    Description = "Player Loss credited to a customer\'s greenzorro account after a 50% deduction. See PlayingLoss for the whole amount."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.FundFee,
                    Description = "Fund fees: 2.5%. Deducted from the customer investment when withdrawing cash."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.GzFees,
                    Description = "greenzorro fees: 1.5%. Deducted from the customer investment when withdrawing cash. Profit for GreenZorro."
                }
                ,
                new GzTrxType {
                    Code = GzTransactionTypeEnum.GzActualTrxProfitOrLoss,
                    Description = "The realized greenzorro profit or loss by the actual purchase of customer shares after the month\'s end. It is the total difference between \"Bought Funds Prices\" * \"Monthly Customer Shares\" - \"Customer Shares\" * \"Funds Prices\" credited to the customer\'s Account."
                }
                );
        }
    }
}