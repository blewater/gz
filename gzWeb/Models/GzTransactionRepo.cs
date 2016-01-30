using System;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace gzWeb.Models {
    public class GzTransactionRepo{

        /// <summary>
        /// Create or update in a idempotent fashion. Multiple calls with same parameters result in one transaction being called.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public async Task AddGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC) {

            using (var db = new ApplicationDbContext()) {

                //Not thread safe but ok...within a single request context
                db.GzTransactions.AddOrUpdate(
                    // Assume CreatedOnUTC remains constant for same trx
                    // to support idempotent transactions
                    t => new { t.CustomerId, t.TypeId, t.CreatedOnUTC, t.Amount },
                        new GzTransaction {
                            CustomerId = customerId,
                            TypeId = db.GzTransationTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                            YearMonthCtd = createdOnUTC.Year.ToString("0000")+createdOnUTC.Month.ToString("00"),
                            Amount = amount,
                            CreatedOnUTC = createdOnUTC
                        }
                    );
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create or update a playing loss. Resulting in atomic 2 row being created. A type of PlayingLoss, CreditedPlayingLoss.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="creditPcnt">Percentage number (0-100) to credit balance. For example 50 for half the amount to be credited</param>
        /// <param name="createdOnUTC">Date of the transaction in UTC</param>
        /// <returns></returns>
        public async Task AddPlayingLoss(int customerId, decimal totPlayinLossAmount, float creditPcnt, DateTime createdOnUTC) {

            if (creditPcnt < 0 || creditPcnt > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + creditPcnt);

            } else {

                using (var db = new ApplicationDbContext()) {
                    using (var dbContextTransaction = db.Database.BeginTransaction()) {

                        try {

                            await AddGzTransaction(customerId, TransferTypeEnum.PlayingLoss, totPlayinLossAmount, createdOnUTC);
                            await AddGzTransaction(customerId, TransferTypeEnum.CreditedPlayingLoss, totPlayinLossAmount * new decimal(creditPcnt / 100), createdOnUTC);

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
   }
}