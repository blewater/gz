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
        public int CustomerId { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique = true, Order=2)]
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
        public decimal? CashBalance { get; set; }

        /// <summary>
        /// Query optimization. Save the portfolio risk type used to calculate this balance.
        /// </summary>
        [Required]
        public int PortfolioRiskEnum { get; set; } = (int) RiskToleranceEnum.Medium;

        /// <summary>
        /// The positive or negative difference compared to the last month.
        /// </summary>
        private decimal invGainLoss = 0;
        public decimal InvGainLoss {
            get { return invGainLoss; }
            set { invGainLoss = value; }
        }

        /// <summary>
        /// The month's cash investment
        /// </summary>
        private decimal cashInvestment = 0;
        public decimal CashInvestment {
            get { return cashInvestment; }
            set { cashInvestment = value; }
        }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}