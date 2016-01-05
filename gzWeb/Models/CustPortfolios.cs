using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class CustPortfolios {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required]
        public int PortfolioId { get; set; }

        [Required, Index]
        public int CustomerId { get; set; }
    }
}