using gzDAL.Models;
using gzDAL.Repos;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using gzDAL.ModelUtil;

namespace gzDAL.Conf
{

    /// <summary>
    /// Seed class moved here from migrations.configuration
    /// Use the line below inside configuration.seed:
    /// 
    ///         Conf.Seed.GenData();
    /// 
    /// If need be here's
    /// how to Truncate tables
    /// http://stackoverflow.com/questions/24209220/ef6-code-first-drop-tables-not-entire-database-when-model-changes
    /// </summary>
    public class Seed {

        public static string GenData() {

            //Assume success
            string retExceptionVal = null;

            // Tracing log location + name i.e. C:\Users\Mario\AppData\Local\Temp\sqlLogFile.log
            var tempSQLLogPath = Path.GetTempPath() + "sqlLogFile.log";

            //http://stackoverflow.com/questions/815586/entity-framework-using-transactions-or-savechangesfalse-and-acceptallchanges
            using (var sqlLogFile = new StreamWriter(tempSQLLogPath)) {
                using (ApplicationDbContext context = new ApplicationDbContext()) {
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
        private static void AddUpdData(ApplicationDbContext context) {

            context.Database.ExecuteSqlCommand("Delete GzTrxs");
            context.Database.ExecuteSqlCommand("Delete CustFundShares");
            context.Database.ExecuteSqlCommand("Delete InvBalances");
            context.Database.ExecuteSqlCommand("Delete FundPrices");
            context.Database.ExecuteSqlCommand("Delete CurrencyListXes");

            // GzConfigurations
            CreateUpdConfiguationRow(context);

            // Customers
            var manager = new ApplicationUserManager(new CustomUserStore(context),
                                                     new DataProtectionProviderFactory(() => null));
            int custId = SaveDbCreateUser(manager, context, TestUserJoe(manager));
            int evuserCustId = SaveDbCreateUser(manager, context, TestEveryMatrixUser(manager));

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
            //CalcMonthlyBalances(context, custId);
            //context.SaveChanges();
        }

        /// <summary>
        /// Insert or Update the gzConfiguration Constants in a single row
        /// </summary>
        /// <param name="context"></param>
        private static void CreateUpdConfiguationRow(ApplicationDbContext context) {

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
        /// <returns></returns>
        private static int SaveDbCreateUser(ApplicationUserManager manager, ApplicationDbContext context, ApplicationUser newUser) {
            // User
            context.Users.AddOrUpdate(c => new { c.Email }, newUser);
            context.SaveChanges();

            var custId = context.Users.Where(u => u.Email == newUser.Email).Select(u => u.Id).Single();
            return custId;
        }

        private static ApplicationUser TestUserJoe(ApplicationUserManager manager) {

            var newUser = new ApplicationUser() {
                UserName = "joe@mymail.com",
                Email = "joe@mymail.com",
                EmailConfirmed = true,
                FirstName = "Joe",
                LastName = "Smith",
                Currency = "SEK",
                Birthday = new DateTime(1990, 1, 1),
                PasswordHash = manager.PasswordHasher.HashPassword("1q2w3e")
            };
            return newUser;
        }

        private static ApplicationUser TestEveryMatrixUser(ApplicationUserManager manager) {

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
            return newUser;
        }

        /// <summary>
        /// Calculate the monthly balances for a customer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="custId"></param>
        private static void CalcMonthlyBalances(ApplicationDbContext context, int custId) {

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

        private static void CreateUpdCurrenciesList(ApplicationDbContext context) {

            var usd = "USD";

            context.CurrenciesListX.AddOrUpdate(
                c => new { c.From, c.To },
                new CurrencyListX {
                    From = "AUD", UpdatedOnUTC=DateTime.UtcNow
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

        private static void CreateUpdFunds(ApplicationDbContext context) {
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

        private static void CreateUpdFundsPrices(ApplicationDbContext context) {

            // End of 2015
            context.FundPrices.AddOrUpdate(
                f => new {f.FundId, f.YearMonthDay},

                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                },
                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                },
                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSGBX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                },
                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "VBMFX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                },
                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "VSTCX").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                },
                new FundPrice {
                    ClosingPrice = 1F,
                    FundId = context.Funds.Where(f => f.Symbol == "CFA").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20151231",
                    UpdatedOnUTC = DateTime.UtcNow
                }
                );

            var begDateTime = new DateTime(2016, 9, 1, 18, 1, 1);
            var endDateTime = DateTime.UtcNow;

            while (begDateTime < endDateTime) {

                context.FundPrices.AddOrUpdate(
                    f => new { f.FundId, f.YearMonthDay },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "VSGBX").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "VSTCX").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "CFA").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    },
                    new FundPrice {
                        ClosingPrice = 1F,
                        FundId = context.Funds.Where(f => f.Symbol == "VBMFX").Select(f => f.Id).FirstOrDefault(),
                        YearMonthDay = begDateTime.ToStringYearMonth(),
                        UpdatedOnUTC = DateTime.UtcNow
                    }
                    );

                context.SaveChanges();

                begDateTime = begDateTime.AddDays(7);
            }
        }

        private static void CreateUpdPortfolios(ApplicationDbContext context, int custId) {

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

        private static void CreateUpdPortFunds(ApplicationDbContext context) {
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

        private static void CreateUpdGzTransaction(ApplicationDbContext context, int custId) {

            var trxRepo = new GzTransactionRepo(context);

            // Use new API
            trxRepo.SaveDbPlayingLoss(custId, 200, new DateTime(2016, 7, 31, 23, 46, 01));

            // Old implementation before repo
            context.GzTrxs.AddOrUpdate(
                t => new { t.CustomerId, t.CreatedOnUtc },
                // June
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201608",
                    CreatedOnUtc = new DateTime(2016, 8, 31, 23, 47, 32),
                    Amount = new decimal(200),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Skip August
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201609",
                    CreatedOnUtc = new DateTime(2016, 9, 30, 23, 47, 46),
                    Amount = new decimal(200),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Sept
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201610",
                    CreatedOnUtc = new DateTime(2016, 10, 31, 23, 47, 46),
                    Amount = new decimal(200),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Oct
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201611",
                    CreatedOnUtc = new DateTime(2016, 11, 30, 23, 47, 46),
                    Amount = new decimal(200),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Dec Skip Nov either won or did not play
                new GzTrx {
                    CustomerId = custId,
                    YearMonthCtd = "201612",
                    CreatedOnUtc = new DateTime(2016, 12, 31, 23, 47, 12),
                    Amount = new decimal(200),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTrxTypes.Where(t => t.Code == GzTransactionTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdTranxType(ApplicationDbContext context) {
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