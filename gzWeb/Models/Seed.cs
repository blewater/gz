using System;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Identity;

namespace gzWeb.Models {

    /// <summary>
    /// Seed class moved here from migrations.configuration
    /// Use the line below inside configuration.seed:
    /// 
    ///         Models.Seed.GenData();
    /// 
    /// </summary>
    public class Seed {
        public static string GenData() {
            //Assume success
            string retVal = null;

            //http://stackoverflow.com/questions/815586/entity-framework-using-transactions-or-savechangesfalse-and-acceptallchanges
            using (var sqlLogFile = new StreamWriter("d:\\temp\\sqlLogFile.txt")) {
                using (ApplicationDbContext context = new ApplicationDbContext()) {
                    context.Database.Log = sqlLogFile.Write;
                    //using (var dbContextTransaction = context.Database.BeginTransaction()) {
                        try {

                            AddUpdData(context);

                            //dbContextTransaction.Commit();
                        } catch (Exception ex) {
                            var exception = retVal = ex.Message;
                            throw ex;
                            //dbContextTransaction.Rollback();
                        }
                    //}
                }
            }
            return retVal;
        }

        private static void AddUpdData(ApplicationDbContext context) {
            var manager = new ApplicationUserManager(new CustomUserStore(context));

            // Customer
            int custId = CreateUpdUser(manager);
            context.SaveChanges();

            // Funds
            CreateUpdFunds(context);
            context.SaveChanges();

            // TransxTypes
            CreateUpdTranxType(context);
            context.SaveChanges();

            // Transxs
            CreateUpdTransx(context, custId);
            context.SaveChanges();

            // Balances
            CreateBalances(context, custId);
            context.SaveChanges();
        }

        private static void CreateBalances(ApplicationDbContext context, int custId) {
            var yearMonthsGroups = context.Transxes.Where(t => t.CustomerId == custId)
                .OrderBy(t => t.YearMonthCtd)
                .GroupBy(t => t.YearMonthCtd)
                .ToList();
            //yearMonths.Dump();
            var prevMonBal = new decimal(0.00);
            foreach (var g in yearMonthsGroups) {

                var InvAmount = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
                var InvGain = g.Sum(t => t.Type.Code == TransferTypeEnum.InvestmentRet ? t.Amount : 0);
                var WithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);

                var gBalance = prevMonBal + InvAmount + InvGain - WithdrawnAmounts;

                context.InvBalances.AddOrUpdate(
                    b => new { b.CustomerId, b.YearMonthCtd },
                    new InvBalance {
                        Balance = gBalance,
                        CustomerId = custId,
                        YearMonthCtd = g.Key
                    }
                );
                //Now this is the previous month balance
                prevMonBal = gBalance;
            }
        }

        private static void CreateUpdTransx(ApplicationDbContext context, int custId) {
            context.Transxes.AddOrUpdate(
                t => new { t.CustomerId, t.CreatedOnUTC, t.TypeId },
                //March
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201503",
                    CreatedOnUTC = new DateTime(2015, 3, 4, 7, 23, 42),
                    Amount = new decimal(10000),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.Deposit).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201503",
                    CreatedOnUTC = new DateTime(2015, 3, 18, 18, 22, 13),
                    Amount = new decimal(9000),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.Deposit).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201503",
                    CreatedOnUTC = new DateTime(2015, 3, 31, 23, 46, 01),
                    Amount = new decimal(9853),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201503",
                    CreatedOnUTC = new DateTime(2015, 3, 31, 23, 46, 02, 853),
                    Amount = new decimal(4926.5),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201503",
                    CreatedOnUTC = new DateTime(2015, 3, 31, 23, 46, 05),
                    Amount = new decimal(1.232),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.InvestmentRet).Select(t => t.Id).FirstOrDefault(),
                },
                // April
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201504",
                    CreatedOnUTC = new DateTime(2015, 4, 13, 19, 27, 03, 704),
                    Amount = new decimal(3000),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.TransferToGaming).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201504",
                    CreatedOnUTC = new DateTime(2015, 4, 30, 23, 47, 53, 934),
                    Amount = new decimal(2013.48),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201504",
                    CreatedOnUTC = new DateTime(2015, 4, 30, 23, 47, 54, 343),
                    Amount = new decimal(1006.74),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201504",
                    CreatedOnUTC = new DateTime(2015, 4, 30, 23, 49, 05),
                    Amount = new decimal(245.32),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.InvestmentRet).Select(t => t.Id).FirstOrDefault(),
                },
                // May
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201505",
                    CreatedOnUTC = new DateTime(2015, 5, 31, 23, 46, 59),
                    Amount = new decimal(1568.43),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201505",
                    CreatedOnUTC = new DateTime(2015, 5, 31, 23, 47, 12),
                    Amount = new decimal(784.22),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // June
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201506",
                    CreatedOnUTC = new DateTime(2015, 6, 30, 23, 47, 22),
                    Amount = new decimal(2384.22),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201506",
                    CreatedOnUTC = new DateTime(2015, 6, 30, 23, 47, 32),
                    Amount = new decimal(1192.11),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // August Skip July he won
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201508",
                    CreatedOnUTC = new DateTime(2015, 8, 31, 23, 47, 23),
                    Amount = new decimal(584.23),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201508",
                    CreatedOnUTC = new DateTime(2015, 8, 31, 23, 47, 46),
                    Amount = new decimal(292.12),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Sept
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201509",
                    CreatedOnUTC = new DateTime(2015, 9, 30, 23, 47, 23),
                    Amount = new decimal(2943),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201509",
                    CreatedOnUTC = new DateTime(2015, 9, 30, 23, 47, 46),
                    Amount = new decimal(1471.5),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Oct
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201510",
                    CreatedOnUTC = new DateTime(2015, 10, 31, 23, 47, 01),
                    Amount = new decimal(1832.21),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201510",
                    CreatedOnUTC = new DateTime(2015, 10, 31, 23, 47, 46),
                    Amount = new decimal(916.11),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                // Dec Skip Nov either won or did not play
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201512",
                    CreatedOnUTC = new DateTime(2015, 12, 31, 23, 46, 58),
                    Amount = new decimal(3354.03),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
                },
                new Transx {
                    CustomerId = custId,
                    YearMonthCtd = "201512",
                    CreatedOnUTC = new DateTime(2015, 12, 31, 23, 47, 12),
                    Amount = new decimal(1677.02),
                    TypeId = context.TransxTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
                }
                );
        }

        private static void CreateUpdTranxType(ApplicationDbContext context) {
            context.TransxTypes.AddOrUpdate(
                t => t.Code,
                new TransxType {
                    Code = TransferTypeEnum.Deposit,
                    Description = "Customer Bank Deposit"
                },
                new TransxType {
                    Code = TransferTypeEnum.Withdrawal,
                    Description = "Customer withdrawals of any excess funds to their banking account"
                },
                new TransxType {
                    Code = TransferTypeEnum.TransferToGaming,
                    Description = "Customer transfers to gaming account"
                },
                new TransxType {
                    Code = TransferTypeEnum.PlayingLoss,
                    Description = "Losses due to playing in Casino, Betting etc"
                },
                new TransxType {
                    Code = TransferTypeEnum.CreditedPlayingLoss,
                    Description = "Losses credited to players account after a 50% deduction"
                },
                new TransxType {
                    Code = TransferTypeEnum.InvestmentRet,
                    Description = "Any gains by the investment returns"
                },
                new TransxType {
                    Code = TransferTypeEnum.FundFee,
                    Description = "Any fees by the Fund itself i.e. 0.5%"
                },
                new TransxType {
                    Code = TransferTypeEnum.Commission,
                    Description = "Commissions (1.5% = 2.5%) Profit for GreenZorro"
                }
                );
        }

        private static void CreateUpdFunds(ApplicationDbContext context) {
            context.Funds.AddOrUpdate(
                f => f.Symbol,
                new Fund {
                    HoldingName = "iShares MUB", Symbol = "MUB"
                },
                new Fund {
                    HoldingName = "Schwab SCHP", Symbol = "SCHP"
                },
                new Fund {
                    HoldingName = "State Street XLE", Symbol = "XLE"
                },
                new Fund {
                    HoldingName = "Vanguard VEA", Symbol = "VEA"
                },
                new Fund {
                    HoldingName = "Vanguard VIG", Symbol = "VIG"
                },
                new Fund {
                    HoldingName = "Vanguard VTI", Symbol = "VTI"
                },
                new Fund {
                    HoldingName = "Vanguard VWO", Symbol = "VWO"
                }
                );
        }

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

            // Truncate tables
            //http://stackoverflow.com/questions/24209220/ef6-code-first-drop-tables-not-entire-database-when-model-changes

            var fUser = manager.FindByEmail(newUser.Email);
            if (fUser == null) {
                manager.Create(newUser, "1q2w3e");
            } else {
                manager.Update(newUser);
            }

            var custId = manager.FindByEmail(newUser.Email).Id;
            return custId;
        }
    }
}