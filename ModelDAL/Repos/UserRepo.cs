using System;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using Z.EntityFramework.Plus;
using NLog;
using static System.Diagnostics.Debug;

namespace gzDAL.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext db;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="gzTransactionRepo"></param>
        /// <param name="invBalanceRepo"></param>
        public UserRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        ///  
        /// Cache & query user asynchronously
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> GetCachedUserAsync(int userId) {

                var userRow = 
                    await db.Users
                        .Where(u => u.Id == userId)
                        .DeferredSingleOrDefault()
                        .FromCacheAsync(DateTime.UtcNow.AddDays(1));

                return userRow;
        }
    }
}
