using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzDAL.Models {

    /// <summary>
    /// The investment balance for the customer per month per row. Hold gain losses too.
    /// </summary>
    public class InvBalance {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique=true, Order=1)]
        [Index("CustomerId_Only_idx_invbal", IsUnique = false)]
        [Index("IDX_InvBalance_CustomerId_YearMonth_CashInv", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique = true, Order=2)]
        [Index("YearMonth_Only_idx_invbal", IsUnique = false)]
        [Index("IDX_InvBalance_CustomerId_YearMonth_CashInv", IsUnique = true, Order = 2)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        /// <summary>
        /// Investment Balance in $ owned in greenzorro managed funds.
        /// </summary>
        [Required]
        public decimal Balance { get; set; }

        /// <summary>
        /// Optional Cash balance only for the where the portfolio was sold.
        /// </summary>
        public decimal? CashBalance { get; set; } = 0;

        /// <summary>
        /// The positive or negative difference compared to the last month.
        /// </summary>
        public decimal InvGainLoss { get; set; } = 0;

        /// <summary>
        /// The month's cash investment
        /// </summary>
        [Index("IDX_InvBalance_CustomerId_YearMonth_CashInv", IsUnique = true, Order = 3)]
        public decimal CashInvestment { get; set; } = 0;

        [Required]
        public DateTime UpdatedOnUTC { get; set; } = DateTime.UtcNow;
    }
}