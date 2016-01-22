using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {

    public class PortfolioRepository {

        private ApplicationDbContext db = new ApplicationDbContext();

        public IEnumerable<Portfolio> GetAllPortfolios() {

            return db.Portfolios.ToList();
        }
    }
}