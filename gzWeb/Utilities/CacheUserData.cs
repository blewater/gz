using System.Linq;
using gzDAL.Models;
using gzDAL.Repos.Interfaces;

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
        public void Query(int userId) {

            using (var db = new ApplicationDbContext()) {

                ApplicationUser user;
                var summary = _userRepo.GetSummaryData(userId, out user);

                _invBalanceRepo.GetCustomerVintagesSellingValue(user.Id, summary.Vintages.ToList());

                _custPortfolioRepo.GetCustomerPlans(userId);

            }
        }
    }
}