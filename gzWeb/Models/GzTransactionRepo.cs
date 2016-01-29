using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;
using EFExtensions;
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
                    // Hopefully CreatedOnUTC remains constant for same
                    // to support idempotent transactions
                    t => new { t.CustomerId, t.TypeId, t.CreatedOnUTC, t.Amount },
                        new GzTransaction {
                            CustomerId = customerId,
                            TypeId = db.GzTransationTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                            YearMonthCtd = createdOnUTC.Year.ToString()+createdOnUTC.Month.ToString(),
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

            using (var db = new ApplicationDbContext()) {
                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    try {

                        await AddGzTransaction(customerId, TransferTypeEnum.PlayingLoss, totPlayinLossAmount, createdOnUTC);
                        await AddGzTransaction(customerId, TransferTypeEnum.CreditedPlayingLoss, totPlayinLossAmount * new decimal(creditPcnt/100), createdOnUTC);

                        dbContextTransaction.Commit();
                    } catch (Exception ex) {
                        var exception = ex.Message;
                        dbContextTransaction.Rollback();
                        throw ex;
                    }
                }
            }
        }


        public void CreateIdempotentTransactions(List<GzTransaction> transactions) {

            using (var db = new ApplicationDbContext()) {
                db.Upsert<GzTransaction>(transactions);
            }

        }

    }
}