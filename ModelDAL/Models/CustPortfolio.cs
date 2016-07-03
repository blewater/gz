using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// Intermediate object: The Customer Portfolio selection
    /// Phase 1: The row weight is always 100%. 
    /// There's a single portfolio per customer.
    /// Phase 2: Multiple portfolios with varying weights.
    /// </summary>
    public class CustPortfolio {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_custp", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required, StringLength(6)]
        [Index("CustomerId_Mon_idx_custp", IsUnique = true, Order = 2)]
        public string YearMonth { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_custp", IsUnique = true, Order = 3)]
        [Index("IX_CustPortfolio_PortfolioId_Only")]
        public int PortfolioId { get; set; }
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; }

        // Customer
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}