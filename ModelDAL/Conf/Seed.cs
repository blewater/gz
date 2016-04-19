using gzDAL.Models;
using gzDAL.Repos;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;

namespace gzDAL.Conf
{

    /// <summary>
    /// Seed class moved here from migrations.configuration
    /// Use the line below inside configuration.seed:
    /// 
    ///         Models.Seed.GenData();
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
                    // using (var dbContextTransaction = context.Database.BeginTransaction()) {
                        try {

                            AddUpdData(context);

                            //dbContextTransaction.Commit();
                        } catch (Exception ex) {
                            var exception = retExceptionVal = ex.Message;
                            //dbContextTransaction.Rollback();
                            throw;
                        }
                    //}
                }
            }
            return retExceptionVal;
        }

        /// <summary>
        /// Calling AddUpdData on all domain objects.
        /// </summary>
        /// <param name="context"></param>
        private static void AddUpdData(ApplicationDbContext context) {
            
            // GzConfigurations
            CreateUpdConfiguationRow(context);

            // Customers
            var manager = new ApplicationUserManager(new CustomUserStore(context));
            int custId = CreateUpdUser(manager);

            int everyMatrixUserId = 0;
            var everyMatrixUser = manager.FindByEmail("gz@greenzorro.gr");
            if (everyMatrixUser != null)
                everyMatrixUserId = everyMatrixUser.Id;

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

            if (everyMatrixUserId > 0) {
                CreateUpdPortfolios(context, custId);
                context.SaveChanges();
            }

            // Link now a portfolio for this customer
            var custPortfolioRepo = new CustPortfolioRepo(context);
            custPortfolioRepo.SaveDbCustMonthsPortfolioMix(custId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));
            if (everyMatrixUserId > 0) {
                custPortfolioRepo.SaveDbCustMonthsPortfolioMix(everyMatrixUserId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));
            }

            // Portfolios - Funds association table
            CreateUpdPortFunds(context);
            context.SaveChanges();

            // gzTransactionTypes
            CreateUpdTranxType(context);
            context.SaveChanges();

            // gzTransactions
            CreateUpdGzTransaction(context, custId);
            context.SaveChanges();
            if (everyMatrixUserId > 0) {
                CreateUpdGzTransaction(context, everyMatrixUserId);
                context.SaveChanges();
            }

            // Balances
            CalcMonthlyBalances(context, custId);
            context.SaveChanges();
            if (everyMatrixUserId > 0) {
                CalcMonthlyBalances(context, everyMatrixUserId);
                context.SaveChanges();
            }
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
                    LOCK_IN_NUM_DAYS = dbConf.LOCK_IN_NUM_DAYS
                }
            );
        }

        /// <summary>
        /// Add or Update existing test users
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        private static int CreateUpdUser(ApplicationUserManager manager) {
            // User
            Random rnd = new Random();
            int rndPlatformId = rnd.Next(1, int.MaxValue);

            var newUser = new ApplicationUser() {
                UserName = "joe@mymail.com",
                Email = "joe@mymail.com",
                EmailConfirmed = true,
                FirstName = "Joe",
                LastName = "Smith",
                Birthday = new DateTime(1990, 1, 1),
                PlatformCustomerId = rndPlatformId,
                GamBalance = new decimal(4200.54),
                GamBalanceUpdOnUTC = DateTime.Now
            };

            var fUser = manager.FindByEmail(newUser.Email);
            if (fUser == null) {
                manager.Create(newUser, "1q2w3e");
            } else {
                manager.Update(newUser);
            }

            var custId = manager.FindByEmail(newUser.Email).Id;
            return custId;
        }

        /// <summary>
        /// Calculate the monthly balances for a customer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="custId"></param>
        private static void CalcMonthlyBalances(ApplicationDbContext context, int custId) {

            new InvBalanceRepo(context, new CustFundShareRepo(context), new GzTransactionRepo(context))
                .SaveDbCustomerMonthlyBalancesByTrx(custId);

        }

        private static void CreateUpdCurrenciesList(ApplicationDbContext context) {
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
                }
                );
        }

        private static void CreateUpdFunds(ApplicationDbContext context) {
            context.Funds.AddOrUpdate(
                f => f.Symbol,
                new Fund {
                    HoldingName = "iShares MUB", Symbol = "MUB", ThreeYrReturnPcnt = 2.81f, FiveYrReturnPcnt = 5.25f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "Schwab SCHP", Symbol = "SCHP", ThreeYrReturnPcnt = -2.34f, FiveYrReturnPcnt = 2.39f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "State Street XLE", Symbol = "XLE", ThreeYrReturnPcnt = -3.27f, FiveYrReturnPcnt = -0.42f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "Vanguard VEA", Symbol = "VEA", ThreeYrReturnPcnt = 4.5f, FiveYrReturnPcnt = 3.48f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "Vanguard VIG", Symbol = "VIG", ThreeYrReturnPcnt = 11.63f, FiveYrReturnPcnt = 10.52f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "Vanguard VTI", Symbol = "VTI", ThreeYrReturnPcnt = 14.66f, FiveYrReturnPcnt = 12.13f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                },
                new Fund {
                    HoldingName = "Vanguard VWO", Symbol = "VWO", ThreeYrReturnPcnt = -7.17f, FiveYrReturnPcnt = -4.98f, UpdatedOnUTC = new DateTime(2016, 1, 22, 12, 2, 0)
                }
                );
        }

        private static void CreateUpdFundsPrices(ApplicationDbContext context) {
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
                    FundId = context.Funds.Where(f => f.Symbol == "SCHP").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 37.88F,
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 40.02F,
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 79.16F,
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20141231",
                    UpdatedOnUTC = new DateTime(2014, 12, 31, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 81.16F,
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
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
                    FundId = context.Funds.Where(f => f.Symbol == "SCHP").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 41.25F,
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 42.31F,
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 78.19F,
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    YearMonthDay = "20150601",
                    UpdatedOnUTC = new DateTime(2015, 6, 1, 23, 50, 0)
                },
                new FundPrice {
                    ClosingPrice = 81.19F,
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
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

        private static void CreateUpdPortfolios(ApplicationDbContext context, int custId) {

            context.Portfolios.AddOrUpdate(
                p => p.RiskTolerance,
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Low, IsActive = true,
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Low_Medium, IsActive = false,
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Medium, IsActive = true,
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.Medium_High, IsActive = false,
                },
                new Portfolio {
                    RiskTolerance = RiskToleranceEnum.High, IsActive = true
                });

        }

        private static void CreateUpdPortFunds(ApplicationDbContext context) {
            context.PortFunds.AddOrUpdate(
                p => new { p.PortfolioId, p.FundId },
                // LOW
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 8,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
                    Weight = 15,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    Weight = 7,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "SCHP").Select(f => f.Id).FirstOrDefault(),
                    Weight = 25f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 35f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low).Select(p => p.Id).FirstOrDefault(),
                },
                // LOW_MEDIUM
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 26,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 11,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
                    Weight = 8,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    Weight = 6,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "SCHP").Select(f => f.Id).FirstOrDefault(),
                    Weight = 9,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 35f,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Low_Medium).Select(p => p.Id).FirstOrDefault(),
                },
                // MEDIUM
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 33,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 15,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    Weight = 12,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
                    Weight = 6,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 29,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium).Select(p => p.Id).FirstOrDefault(),
                },
                // MEDIUM-HIGH
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 35,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 21,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    Weight = 16,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
                    Weight = 8,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 15,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.Medium_High).Select(p => p.Id).FirstOrDefault(),
                },
                // HIGH
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VTI").Select(f => f.Id).FirstOrDefault(),
                    Weight = 35,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VEA").Select(f => f.Id).FirstOrDefault(),
                    Weight = 22,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VWO").Select(f => f.Id).FirstOrDefault(),
                    Weight = 28,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "VIG").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "XLE").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                },
                new PortFund {
                    FundId = context.Funds.Where(f => f.Symbol == "MUB").Select(f => f.Id).FirstOrDefault(),
                    Weight = 5,
                    PortfolioId = context.Portfolios.Where(p => p.RiskTolerance == RiskToleranceEnum.High).Select(p => p.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdGzTransaction(ApplicationDbContext context, int custId) {

            var trxRepo = new GzTransactionRepo(context);

            // Use new API
            trxRepo.SaveDbGzTransaction(custId, GzTransactionJournalTypeEnum.Deposit, 10000, new DateTime(2015, 3, 4, 7, 23, 42));
            trxRepo.SaveDbGzTransaction(custId, GzTransactionJournalTypeEnum.Deposit, 90000, new DateTime(2015, 3, 18, 18, 22, 13));
            trxRepo.SaveDbPlayingLoss(custId, 9853, 50, new DateTime(2015, 3, 31, 23, 46, 01));

            trxRepo.SaveDbTransferToGamingAmount(custId, 300, new DateTime(2015, 4, 15, 11, 26, 02, 52));
            trxRepo.SaveDbPlayingLoss(custId, 2013, 50, new DateTime(2015, 4, 29, 23, 56, 12, 42));

            // Old implementation before repo
            context.GzTransactions.AddOrUpdate(
                t => new { t.CustomerId, t.CreatedOnUTC },
                // May
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201505",
                    CreatedOnUTC = new DateTime(2015, 5, 31, 23, 46, 59),
                    Amount = new decimal(1568.43),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201505",
                    CreatedOnUTC = new DateTime(2015, 5, 31, 23, 47, 12),
                    Amount = new decimal(784.22),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // June
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201506",
                    CreatedOnUTC = new DateTime(2015, 6, 30, 23, 47, 22),
                    Amount = new decimal(2384.22),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201506",
                    CreatedOnUTC = new DateTime(2015, 6, 30, 23, 47, 32),
                    Amount = new decimal(1192.11),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // August Skip July he won
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201508",
                    CreatedOnUTC = new DateTime(2015, 8, 31, 23, 47, 23),
                    Amount = new decimal(584.23),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201508",
                    CreatedOnUTC = new DateTime(2015, 8, 31, 23, 47, 46),
                    Amount = new decimal(292.12),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Sept
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201509",
                    CreatedOnUTC = new DateTime(2015, 9, 30, 23, 47, 23),
                    Amount = new decimal(2943),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201509",
                    CreatedOnUTC = new DateTime(2015, 9, 30, 23, 47, 46),
                    Amount = new decimal(1471.5),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Oct
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201510",
                    CreatedOnUTC = new DateTime(2015, 10, 31, 23, 47, 01),
                    Amount = new decimal(1832.21),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201510",
                    CreatedOnUTC = new DateTime(2015, 10, 31, 23, 47, 46),
                    Amount = new decimal(916.11),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Dec Skip Nov either won or did not play
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201512",
                    CreatedOnUTC = new DateTime(2015, 12, 31, 23, 46, 58),
                    Amount = new decimal(3354.03),
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new GzTransaction {
                    CustomerId = custId,
                    YearMonthCtd = "201512",
                    CreatedOnUTC = new DateTime(2015, 12, 31, 23, 47, 12),
                    Amount = new decimal(1677.02),
                    CreditPcntApplied = 50,
                    TypeId = context.GzTransationTypes.Where(t => t.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdTranxType(ApplicationDbContext context) {
            context.GzTransationTypes.AddOrUpdate(
                t => t.Code,
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.Deposit,
                    Description = "Customer Bank Deposit."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.InvWithdrawal,
                    Description = "Customer withdrawals of any excess funds to their banking account."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.TransferToGaming,
                    Description = "Customer transfers to gaming account."
                }, 
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.CasinoWithdrawal,
                    Description = "Losses due to playing in Casino, Betting etc."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.PortfolioLiquidation,
                    Description = "Liquidating or converting the customer portfolio to cash."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.PlayingLoss,
                    Description = "Losses due to playing in Casino, Betting etc."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.CreditedPlayingLoss,
                    Description = "Losses credited to players account after a 50% deduction."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.FundFee,
                    Description = "Any fees by the Fund itself i.e. 0.5%."
                },
                new GzTransactionType {
                    Code = GzTransactionJournalTypeEnum.GzFees,
                    Description = "Commissions (1.5% = 2.5%) Profit for GreenZorro."
                }
                );
        }
    }
}