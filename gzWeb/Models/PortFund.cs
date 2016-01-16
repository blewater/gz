using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    /// <summary>
    /// Bridge for Many to Many Portfolio <--> Fund
    /// Many to Many with additional fields in association table
    /// http://stackoverflow.com/questions/7050404/create-code-first-many-to-many-with-additional-fields-in-association-table
    /// </summary>
    public class PortFund {

        [Required, Key, Column(Order = 0)]
        public int PortfolioId { get; set; }

        [Required, Key, Column(Order = 1)]
        public int FundId { get; set; }

        public virtual Portfolio Portfolio { get; set; }
        public virtual Fund Fund { get; set; }

        [Required]
        public float Weight { get; set; }
    }
}