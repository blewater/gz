using System;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace gzWeb.Models {

    /// <summary>
    /// For any Greenzorro Transaction creation/update
    /// Currency conversions are encapsulated here alone.
    /// </summary>
    public class GzTransactionRepo {

        /// <summary>
        /// Greenzorro percentage fee %
        /// </summary>
        const decimal COMMISSION_PCNT = 1.5M;

        /// <summary>
        /// Fund fee %
        /// </summary>
        const decimal FUND_FEE_PCNT = 2.5M;

        /// <summary>
        /// Percentage to credit on loss
        /// </summary>
        const decimal CREDIT_LOSS_PCNT = 50M;

        /// <summary>
        /// Create any type of transaction from those allowed.
        /// Used along with peer API methods for specialized transactions
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public async Task AddGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC) {

            if (
                       gzTransactionType == TransferTypeEnum.GzFees
                    || gzTransactionType == TransferTypeEnum.FundFee
                    || gzTransactionType == TransferTypeEnum.InvWithdrawal

                    || gzTransactionType == TransferTypeEnum.PlayingLoss
                    || gzTransactionType == TransferTypeEnum.CreditedPlayingLoss ) {

                throw new Exception("This type of transaction can be created/updated only by the specialized api of this class" + amount);
            }

            if (amount < 0) {

                throw new Exception("Amount must be greater than 0: " + amount);

            }

            using (var db = new ApplicationDbContext()) {
                await SaveDBTransaction(customerId, gzTransactionType, amount, createdOnUTC, db);
            }
        }

        /// <summary>
        /// Transfer to the Gaming account by selling investment shares and save the calculated commission and fund fees transactions
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public async Task AddTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC) {

            if (withdrawnAmount < 0) {

                throw new Exception("Invalid amount to transfer to gaming account. Amount must be greater than 0: " + withdrawnAmount);

            } else {

                using (var db = new ApplicationDbContext()) {
                    using (var dbContextTransaction = db.Database.BeginTransaction()) {

                        try {

                            await SaveDBTransaction(customerId, TransferTypeEnum.TransferToGaming, withdrawnAmount, createdOnUTC, db);
                            await SaveDBGreenZorroFees(customerId, withdrawnAmount, createdOnUTC, db);

                            dbContextTransaction.Commit();
                        } catch (Exception ex) {
                            var exception = ex.Message;
                            dbContextTransaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save an investment withdrawal transaction and save the calculated commission and fund fees transactions
        /// Note identical method to <see cref="AddTransferToGamingAmount"/> though in practice it may not be 
        /// able to instruct the casino platform to use it.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public async Task AddInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC) {

            if (withdrawnAmount < 0) {

                throw new Exception("Invalid withdrawal. Amount must be greater than 0: " + withdrawnAmount);

            } else {

                using (var db = new ApplicationDbContext()) {
                    using (var dbContextTransaction = db.Database.BeginTransaction()) {

                        try {

                            await SaveDBTransaction(customerId, TransferTypeEnum.InvWithdrawal, withdrawnAmount, createdOnUTC, db);
                            await SaveDBGreenZorroFees(customerId, withdrawnAmount, createdOnUTC, db);

                            dbContextTransaction.Commit();
                        } catch (Exception ex) {
                            var exception = ex.Message;
                            dbContextTransaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save to DB the calculated Fund Greenzorro fees
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="investmentSellOffAmount"></param>
        /// <param name="createdOnUTC"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task SaveDBGreenZorroFees(int customerId, decimal investmentSellOffAmount, DateTime createdOnUTC, ApplicationDbContext db) {
            await SaveDBTransaction(customerId, TransferTypeEnum.GzFees, investmentSellOffAmount * COMMISSION_PCNT / 100, createdOnUTC, db);
            await SaveDBTransaction(customerId, TransferTypeEnum.FundFee, investmentSellOffAmount * FUND_FEE_PCNT / 100, createdOnUTC, db);
        }

        /// <summary>
        /// Create or update a playing loss. Resulting in atomic 2 row being created. A type of PlayingLoss, CreditedPlayingLoss.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="creditPcnt">Percentage number (0-100) to credit balance. For example 50 for half the amount to be credited</param>
        /// <param name="createdOnUTC">Date of the transaction in UTC</param>
        /// <returns></returns>
        public async Task AddPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUTC) {

            if (creditPcnt < 0 || creditPcnt > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + creditPcnt);

            } else {

                using (var db = new ApplicationDbContext()) {
                    using (var dbContextTransaction = db.Database.BeginTransaction()) {

                        try {

                            await SaveDBTransaction(customerId, TransferTypeEnum.PlayingLoss, totPlayinLossAmount, createdOnUTC, db);
                            await SaveDBTransaction(customerId, TransferTypeEnum.CreditedPlayingLoss, totPlayinLossAmount * creditPcnt / 100, createdOnUTC, db);

                            dbContextTransaction.Commit();
                        } catch (Exception ex) {
                            var exception = ex.Message;
                            dbContextTransaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Using an existing DbContext (to support transactions)
        /// Save any transaction type to the database
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUTC"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task SaveDBTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC, ApplicationDbContext db) {
            //Not thread safe but ok...within a single request context
            db.GzTransactions.AddOrUpdate(
                // Assume CreatedOnUTC remains constant for same trx
                // to support idempotent transactions
                t => new { t.CustomerId, t.TypeId, t.CreatedOnUTC, t.Amount },
                    new GzTransaction {
                        CustomerId = customerId,
                        TypeId = db.GzTransationTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = createdOnUTC.Year.ToString("0000") + createdOnUTC.Month.ToString("00"),
                        Amount = amount,
                        CreatedOnUTC = createdOnUTC
                    }
                );
            await db.SaveChangesAsync();
        }

    }
}