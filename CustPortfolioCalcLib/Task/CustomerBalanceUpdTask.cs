using System;
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
        private readonly ApplicationDbContext _Db;
        private readonly IInvBalanceRepo _invBalanceRepo;

        private int[] _customerIds = new int[0];
        public int[] CustomerIds
        {
            get { return _customerIds; }
            set { _customerIds = value; }
        }

        public CustomerBalanceUpdTask(ApplicationDbContext db, InvBalanceRepo invBalanceRepo) {

            this._Db = db;
            this._invBalanceRepo = invBalanceRepo;

        }

        /// <summary>
        /// YYYYMM format i.e. 201603 (March of 2016)
        /// </summary>
        private string[] _yearMonthsToProc = new string[0];
        public string[] YearMonthsToProc {
            get { return _yearMonthsToProc; }
            set { _yearMonthsToProc = value; }
        }

        public override void DoTask() {

            // Process the world
            if (CustomerIds.Length == 0 && YearMonthsToProc.Length == 0) {

                _invBalanceRepo.SaveDbAllCustomersMonthlyBalances();
            }

            // Process all customers single month
            else if (CustomerIds.Length == 0) {

                foreach (var yearMonth in YearMonthsToProc) {

                    _invBalanceRepo.SaveDbAllCustomersMonthlyBalances(yearMonth, yearMonth);

                }

            // Process all months for a given customer
            } else {

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

            // Process all given month
            if (YearMonthsToProc.Length > 0) {
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