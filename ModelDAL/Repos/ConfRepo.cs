using System;
using System.Collections.Generic;
using System.Linq;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos
{
    public class ConfRepo : IConfRepo
    {
        private readonly ApplicationDbContext _db;
        public ConfRepo(ApplicationDbContext db)
        {
            this._db = db;
        }

        /// <summary>
        /// 
        /// Get the singleton configuration row
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<GzConfiguration> GetConfRow() {

            var cachedGzConf = await _db.GzConfigurations
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));

            var gzConf = 
                cachedGzConf.Select(c => c)
                .Single();
             
            return gzConf;
        }
    }
}