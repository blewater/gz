using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;


namespace gzWeb.Models {
    public class CustPortfolioRepo {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="weight">Percentage weight for example 33.3 for 1/3 of the balance</param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public async Task SetMonthCustPortfolio(int customerId, RiskToleranceEnum riskType, float weight, DateTime UpdatedOnUTC) {

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
                                YearMonthCtd = UpdatedOnUTC.Year.ToString("0000") + UpdatedOnUTC.Month.ToString("00"),
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