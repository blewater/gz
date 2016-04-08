using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    /// <summary>
    ///
    /// Update monthly Investment Balances for Customer
    ///
    /// </summary>
    public class CustInvestmentBalUpd : CpcTask {

        public int[] CustomerIds { get; set; }

        /// <summary>
        /// YYYYMM format i.e. 201603 (March of 2016)
        /// </summary>
        public string[] YearMonthsToProc { get; set; }

        public override void DoTask() {

            var db = new ApplicationDbContext();
            new InvBalanceRepo(db, new CustFundShareRepo(db), new GzTransactionRepo(db))
                .SaveDbCustomersMonthlyBalancesByTrx(CustomerIds, YearMonthsToProc);

        }
    }
}