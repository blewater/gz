using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;

namespace gzCpcLib.Task {

    /// <summary>
    ///
    /// Update monthly Investment Balances for Customer
    ///
    /// </summary>
    public class CustomerBalanceUpdTask : CpcTask {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IInvBalanceRepo _invBalanceRepo;

        public List<int> CustomerIds { private get; set; } = new List<int>();

        /// <summary>
        /// 
        /// Default constructor using default connection string for the debug or release mode project configuration
        /// 
        /// </summary>
        public CustomerBalanceUpdTask() {

            var db = new ApplicationDbContext();
            InitializeHelperRepos(db);

        }

        /// <summary>
        /// 
        /// Explicit Connection String Constructor
        /// 
        /// </summary>
        public CustomerBalanceUpdTask(bool isProd) {

            var db = new ApplicationDbContext(isProd?"gzProdDb":"gzDevDb");
            InitializeHelperRepos(db);

        }

        private void InitializeHelperRepos(ApplicationDbContext db) {

            var custPortfolio = new CustPortfolioRepo(db);
            this._invBalanceRepo = new InvBalanceRepo(db, new CustFundShareRepo(db, custPortfolio), new GzTransactionRepo(db),
                custPortfolio);
        }

        /// <summary>
        /// YYYYMM format i.e. 201603 (March of 2016)
        /// </summary>
        public List<string> YearMonthsToProc { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// Process last month for all customers
        /// 
        /// </summary>
        public override void DoTask() {

            var lastMonth = DateTime.UtcNow.AddMonths(-1).ToStringYearMonth();
            YearMonthsToProc.Add(lastMonth);
            ProcessSelectively();

        }

        /// <summary>
        /// 
        /// Process based on this class instance properties
        /// 
        /// 1. The World: All Customers for all months (No properties are set)
        /// 
        /// -- or 
        /// 
        /// 2. Last month for all or few/any customers (YearMonthsToProc is set and CustomerIds may optionally be set)
        /// 
        /// -- or 
        /// 
        /// 3. One Customer for all or few/any months (CustomerIds is set and YearMonthsToProc may optionally be set)
        /// 
        /// </summary>
        public void ProcessSelectively() {

// Process the world
            if (CustomerIds.Count == 0 && YearMonthsToProc.Count == 0) {

                _invBalanceRepo.SaveDbAllCustomersMonthlyBalances();
            }

            // Process all customers single month
            else if (CustomerIds.Count == 0) {

                foreach (var yearMonth in YearMonthsToProc) {

                    _invBalanceRepo.SaveDbAllCustomersMonthlyBalances(yearMonth, yearMonth);

                }

                // Process all months for a given customer
            }
            else {

                foreach (var customerId in CustomerIds) {
                    SaveDbProcessCustomerMonthlyUpdates(customerId);
                }
            }
        }

        /// <summary>
        /// 
        /// Process for a given customer all months or just the current month
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        private void SaveDbProcessCustomerMonthlyUpdates(int customerId) {

            // Process all given months
            if (YearMonthsToProc.Count > 0) {
                foreach (var yearMonth in YearMonthsToProc) {

                    _invBalanceRepo.SaveDbCustomerMonthlyBalance(customerId, yearMonth);
                }
            }
            else {
                // Process present month
                _invBalanceRepo.SaveDbCustomerMonthlyBalance(customerId, DateTime.UtcNow.ToStringYearMonth());
            }
        }
    }
}