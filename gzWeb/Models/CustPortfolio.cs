using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class CustPortfolio {
        [Required, Key, Column(Order = 0)]
        public int CustomerId { get; set; }

        [Required, Key, Column(Order = 1)]
        public int PortfolioId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Portfolio Portfolio { get; set; }

        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}