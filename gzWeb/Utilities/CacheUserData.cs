using System;
using System.Linq;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Repos.Interfaces;
using NLog;

namespace gzWeb.Utilities {

    /// <summary>
    /// 
    /// Query time intensive functions asynchronously that cache their results.
    /// 
    /// </summary>
    public class CacheUserData : ICacheUserData {

        private readonly IInvBalanceRepo _invBalanceRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICustPortfolioRepo _custPortfolioRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CacheUserData(IInvBalanceRepo invBalanceRepo, ICustPortfolioRepo custPortfolioRepo, IUserRepo userRepo) {

            _invBalanceRepo = invBalanceRepo;
            _custPortfolioRepo = custPortfolioRepo;
            _userRepo = userRepo;

        }

        /// <summary>
        /// 
        /// Cash user data by calling queries with async cache options
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public async Task Query(int userId) {

            try {

                var summaryRes = await _userRepo.GetSummaryDataAsync(userId);

                _invBalanceRepo.GetCustomerVintagesSellingValue(summaryRes.Item2.Id, summaryRes.Item1.Vintages.ToList());

                _custPortfolioRepo.GetCustomerPlans(userId);

            }
            catch (Exception ex) {
                _logger.Error(ex, "Exception in Query()");
            }
        }
    }
}