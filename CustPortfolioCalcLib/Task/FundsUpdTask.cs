﻿using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    /// <summary>
    /// 
    /// Request latest stock market pricing info for the interested Funds
    /// 
    /// </summary>
    public class FundsUpdTask : CpcTask {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void DoTask() {

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDbDailyFundClosingPrices();
        }
    }
}