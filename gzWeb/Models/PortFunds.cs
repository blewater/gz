using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    /// <summary>
    /// Bridge for Many to Many Portfolio <--> Fund
    /// </summary>
    public class PortFunds {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required, Index]
        public int PortfolioId { get; set; }

        [Required]
        public int FundId { get; set; }
    }
}