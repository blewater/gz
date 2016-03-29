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

        public override void DoTask() {

            var db = new ApplicationDbContext();
            new InvBalanceRepo(db, new CustFundShareRepo(db)).SaveCustTrxsBalances(CustomerIds);

        }
    }
}