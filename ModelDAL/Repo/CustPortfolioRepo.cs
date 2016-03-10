using System;
using System.Linq;
using System.Data.Entity.Migrations;
using gzWeb.Model.Util;
using gzWeb.Repo.Interfaces;
using gzWeb.Models;

namespace gzWeb.Repo {
    public class CustPortfolioRepo : ICustPortfolioRepo
    {

        /// <summary>
        /// Phase 1 Implementation
        /// Save the month's full portfolio mix for a customer.
        /// For phase I you can only select 1 portfolio in 100%
        /// V2 will accept a collection of portfolios for example, 30% for Low, 50% Medium, 20% High
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="portfYear">i.e. 2015 the year this portfolio weight applies</param>
        /// <param name="portfMonth">1..12 the month this portfolio weight applies</param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public void SaveDBCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (customerId <= 0) {

                throw new Exception("Invalid Customer Id: " + customerId);
            }
            DateTime monthsPortfolio = new DateTime(portfYear, portfMonth, DateTime.UtcNow.Day);

            var monthDifffromNow = DbExpressions.MonthDiff(DateTime.UtcNow, monthsPortfolio);

            if (monthDifffromNow != 0) {

                throw new Exception("You can set the customer portfolio for the present month only. Passed in a past month: " + monthsPortfolio);

            }

            this.SaveDBCustMonthsPortfolioMix(customerId, riskType, 100, portfYear, portfMonth, UpdatedOnUTC);
        }

        /// <summary>
        /// Phase 2 Implementation allowing multiple portfolios linked to a *single* customer with a weight mix.
        /// Note Does not impose present month restriciton. Can set a portfolio mix for the past.
        /// Used internally by Phase 1 method for 100% single customer portfolio selection and for Seeding outside of the current month.
        /// Use overloaded SetCustMonthsPortfolioMix instead for phase I
        /// Though it's use is internal it's declared Public to unit test it.
        /// Save the month's portfolio mix for a customer.
        /// For example, 30% for Low, 50% Medium, 20% High
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="weight">Percentage weight for example 33.3 for 1/3 of the balance</param>
        /// <param name="portfYear">i.e. 2015 the year this portfolio weight applies</param>
        /// <param name="portfMonth">1..12 the month this portfolio weight applies</param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public void SaveDBCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (weight < 0 || weight > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + weight);

            } else {

                using (var db = new ApplicationDbContext()) {

                    //Not thread atomic but ok...within a single request context
                    db.CustPortfolios.AddOrUpdate(
                        // Assume UpdatedOnUTC remains constant for same trx
                        // to support idempotent transactions
                        cp => new { cp.CustomerId, cp.YearMonth },
                            new CustPortfolio {
                                CustomerId = customerId,
                                PortfolioId = db.Portfolios.Where(p => p.RiskTolerance == riskType).Select(p => p.Id).FirstOrDefault(),
                                YearMonth = DbExpressions.GetStrYearMonth(portfYear, portfMonth),
                                UpdatedOnUTC = UpdatedOnUTC,
                                Weight = weight
                            }
                        );
                    db.SaveChanges();
                }
            }
        }
    }
}