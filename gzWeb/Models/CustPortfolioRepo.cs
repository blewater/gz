using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;


namespace gzWeb.Models {
    public class CustPortfolioRepo {

        /// <summary>
        /// Save the month's full portfolio mix for a customer.
        /// For phase I you can only select 1 portfolio in 100%
        /// V2 will accept a collection of portfolios for example, 30% for Low, 50% Medium, 20% High
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="portfYear"></param>
        /// <param name="portfMonth"></param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public async Task SetCustMonthsFullPortfolio(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (customerId <= 0) {

                throw new Exception("Invalid Customer Id: " + customerId);
            }
            DateTime monthsPortfolio = new DateTime(portfYear, portfMonth, DateTime.UtcNow.Day);

            //http://stackoverflow.com/questions/4638993/difference-in-months-between-two-dates
            var monthDifffromNow = ((DateTime.UtcNow.Year - monthsPortfolio.Year) * 12) + DateTime.UtcNow.Month - monthsPortfolio.Month;

            if (monthDifffromNow != 0) {

                throw new Exception("You can set the customer portfolio only for the current month: " + monthsPortfolio);

            }

            await this.SetCustMonthsPortfolio(customerId, riskType, 100, portfYear, portfMonth, UpdatedOnUTC);
        }

        /// <summary>
        /// Use SetCustMonthsFullPortfolio instead
        /// Public in order to unit test it.
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
        public async Task SetCustMonthsPortfolio(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (weight < 0 || weight > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + weight);

            } else {

                using (var db = new ApplicationDbContext()) {

                    //Not thread safe but ok...within a single request context
                    db.CustPortfolios.AddOrUpdate(
                        // Assume UpdatedOnUTC remains constant for same trx
                        // to support idempotent transactions
                        cp => new { cp.CustomerId, cp.PortfolioId, cp.UpdatedOnUTC },
                            new CustPortfolio {
                                CustomerId = customerId,
                                PortfolioId = db.Portfolios.Where(p => p.RiskTolerance == riskType).Select(p => p.Id).FirstOrDefault(),
                                YearMonth = portfYear.ToString("0000") + portfMonth.ToString("00"),
                                UpdatedOnUTC = UpdatedOnUTC,
                                Weight = weight
                            }
                        );
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}