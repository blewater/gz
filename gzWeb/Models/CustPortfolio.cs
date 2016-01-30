using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class CustPortfolio {
        [Required, Key, Column(Order = 0)]
        public int CustomerId { get; set; }

        [Required, Key, Column(Order = 1), StringLength(6)]
        public string YearMonthCtd { get; set; }

        // Customer
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public int PortfolioId { get; set; }
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}