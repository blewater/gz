using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.DTO
{

    /// <summary>
    /// Customer Values buffer for values communication between controller views
    /// </summary>
    public class CustomerDTO {

        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public int PlatformCustomerId { get; set; }
        public decimal InvBalance { get; set; }
        public decimal GamBalance { get; set; }

        public decimal LastInvestmentAmount { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalInvestmReturns { get; set; }
        public decimal TotalInvestments { get; set; }
    }
}