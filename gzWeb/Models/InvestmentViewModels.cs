using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    public class CustomerViewModel {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public decimal InvBalance { get; set; }

        public decimal GamBalance { get; set; }

        public decimal LastInvestmentAmount { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalInvestmReturns { get; set; }
        public decimal TotalInvestments { get; set; }
    }
}