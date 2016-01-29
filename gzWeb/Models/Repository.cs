using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    abstract public class Repository {

        protected ApplicationDbContext db;

        public Repository() {

            db = new ApplicationDbContext();
        }

    }
}