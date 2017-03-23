using System;
using System.Collections.Generic;
using System.Linq;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using System.Runtime.Caching;
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

            string key = "gzConfiguration";
            var gzConfiguration = (GzConfiguration)MemoryCache.Default.Get(key);

            if (gzConfiguration == null) {
                var cachedGzConf = await _db.GzConfigurations
                    .FromCacheAsync(DateTime.UtcNow.AddDays(1));

                gzConfiguration =
                    cachedGzConf.Select(c => c)
                        .Single();

                // 1 day cache
                MemoryCache
                    .Default
                    .Set(key, gzConfiguration, DateTimeOffset.UtcNow.AddDays(1));
            }


            return gzConfiguration;
        }
    }
}