using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace gzWeb.Models {

    public class PortfolioRepository {

        private ApplicationDbContext db = new ApplicationDbContext();

        public IQueryable<Portfolio> GetAllPortfolios() {

            return db.Portfolios;
        }
    }
}