using System;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using System.IO;
using gzWeb.Model.Util;
using gzWeb.Repo.Interfaces;
using gzWeb.Models;

namespace gzWeb.Repo {

    /// <summary>
    /// For any Greenzorro Transaction creation/update
    /// Currency conversions are encapsulated here alone.
    /// </summary>
    public class GzTransactionRepo : IGzTransactionRepo
    {

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

        private readonly ApplicationDbContext db;
        public GzTransactionRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Create any type of transaction from those allowed.
        /// Used along with peer API methods for specialized transactions
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public void SaveDBGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC) {

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

            
            SaveDBGzTransaction(customerId, gzTransactionType, amount, createdOnUTC, null);
            
        }

        /// <summary>
        /// Transfer to the Gaming account by selling investment shares and save the calculated commission and fund fees transactions
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public void SaveDBTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC) {

            if (withdrawnAmount < 0) {

                throw new Exception("Invalid amount to transfer to gaming account. Amount must be greater than 0: " + withdrawnAmount);

            } else {
                //string datPath = "d:\\temp";
                //using (var sqlLogFile = new StreamWriter(datPath + "\\sqlLogFile_SaveDBTransferToGamingAmount.log")) {

                    

                        //db.Database.Log = sqlLogFile.Write;

                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    SaveDBGzTransaction(customerId, TransferTypeEnum.TransferToGaming, withdrawnAmount, createdOnUTC, null);
                    SaveDBGreenZorroFees(customerId, withdrawnAmount, createdOnUTC);

                    dbContextTransaction.Commit();
                    
                }
                    
                //}
            }
        }

        /// <summary>
        /// Save an investment withdrawal transaction and save the calculated commission and fund fees transactions
        /// Note identical method to <see cref="SaveDBTransferToGamingAmount"/> though in practice it may not be 
        /// able to instruct the casino platform to use it.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUTC"></param>
        /// <returns></returns>
        public void SaveDBInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC) {

            if (withdrawnAmount < 0) {

                throw new Exception("Invalid withdrawal. Amount must be greater than 0: " + withdrawnAmount);

            } else {

                
                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    SaveDBGzTransaction(customerId, TransferTypeEnum.InvWithdrawal, withdrawnAmount, createdOnUTC, null);
                    SaveDBGreenZorroFees(customerId, withdrawnAmount, createdOnUTC);

                    dbContextTransaction.Commit();
                        
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
        private void SaveDBGreenZorroFees(int customerId, decimal investmentSellOffAmount, DateTime createdOnUTC) {
            SaveDBGzTransaction(customerId, TransferTypeEnum.GzFees, investmentSellOffAmount * COMMISSION_PCNT / 100, createdOnUTC, null);
            SaveDBGzTransaction(customerId, TransferTypeEnum.FundFee, investmentSellOffAmount * FUND_FEE_PCNT / 100, createdOnUTC, null);
        }

        /// <summary>
        /// Create or update a playing loss. Resulting in atomic 2 row being created. A type of PlayingLoss, CreditedPlayingLoss.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="creditPcnt">Percentage number (0-100) to credit balance. For example 50 for half the amount to be credited</param>
        /// <param name="createdOnUTC">Date of the transaction in UTC</param>
        /// <returns></returns>
        public void SaveDBPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUTC) {

            if (creditPcnt < 0 || creditPcnt > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + creditPcnt);

            } else {

                
                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    SaveDBGzTransaction(customerId, TransferTypeEnum.PlayingLoss, totPlayinLossAmount, createdOnUTC, null);
                    SaveDBGzTransaction(customerId, TransferTypeEnum.CreditedPlayingLoss, totPlayinLossAmount * creditPcnt / 100, createdOnUTC, (float) CREDIT_LOSS_PCNT);

                    dbContextTransaction.Commit();
                }
                
            }
        }

        /// <summary>
        /// Save to database a general type of transation using an existing DbContext (to support transactions)
        /// Save any transaction type to the database
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUTC"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private void SaveDBGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC, float? creditPcntApplied) {
            //Not thread safe but ok...within a single request context
            db.GzTransactions.AddOrUpdate(
                // Assume CreatedOnUTC remains constant for same trx
                // to support idempotent transactions
                t => new { t.CustomerId, t.TypeId, t.CreatedOnUTC },
                    new GzTransaction {
                        CustomerId = customerId,
                        TypeId = db.GzTransationTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = createdOnUTC.Year.ToString("0000") + createdOnUTC.Month.ToString("00"),
                        Amount = amount,
                        // Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions
                        CreditPcntApplied = creditPcntApplied,
                        // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                        CreatedOnUTC = DbExpressions.Truncate(createdOnUTC, TimeSpan.FromSeconds(1))
                    }
                );
            db.SaveChanges();
        }

    }
}