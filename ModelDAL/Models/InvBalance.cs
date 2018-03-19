using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// The investment balance for the customer per month per row. Hold monthly gain or losses.
    /// </summary>
    public class InvBalance {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique=true, Order=1)]
        [Index("CustomerId_Only_idx_invbal", IsUnique = false)]
        [Index("IDX_InvBalance_Cust_YM_Sold", IsUnique = true, Order = 1)]
        [Index("IDX_InvBalance_Cust_SoldYM_Sold", Order = 1)]
        [Index("IDX_InvBalance_Cust_YM_CashInv", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique = true, Order=2)]
        [Index("YearMonth_Only_idx_invbal", IsUnique = false)]
        [Index("IDX_InvBalance_Cust_YM_Sold", IsUnique = true, Order = 2)]
        [Index("IDX_InvBalance_Cust_YM_CashInv", IsUnique = true, Order = 2)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Required]
        public int PortfolioId { get; set; }
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; }

        #region Balance Amounts

        /// <summary>
        /// Monthly Starting Gaming balance imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal BegGmBalance { get; set; }

        /// <summary>
        /// Monthly deposits imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal Deposits { get; set; }

        [DefaultValue(0)]
        [Required]
        public decimal CashBonusAmount { get; set; }

        [DefaultValue(0)]
        [Required]
        public decimal CashDeposits { get; set; }

        /// <summary>
        /// Monthly withdrawals imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal Withdrawals { get; set; }

        /// <summary>
        /// Monthly net gain or losses imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal GmGainLoss { get; set; }

        /// <summary>
        /// Monthly Ending Gaming balance imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal EndGmBalance { get; set; }

        /// <summary>
        /// Investment Balance in registered user currency owned in greenzorro managed funds.
        /// </summary>
        [Required]
        public decimal Balance { get; set; }

        #endregion
        /// <summary>
        /// The month's cash investment or the credited player's loss to buy stock for
        /// </summary>
        [Index("IDX_InvBalance_Cust_YM_CashInv", IsUnique = true, Order = 3)]
        [DefaultValue(0)]
        public decimal CashInvestment { get; set; } = 0;

        /// <summary>
        /// Total cash investment of previous months; used in investment gain and direct queries.
        /// </summary>
        [DefaultValue(0)]
        public decimal TotalCashInvInHold { get; set; } = 0;

        /// <summary>
        /// Total cash investment of previous months; used in investment gain and direct queries.
        /// </summary>
        [DefaultValue(0)]
        public decimal TotalCashInvestments { get; set; } = 0;

        /// <summary>
        /// Sum of all previous vintage selling value
        /// </summary>
        [Required]
        [DefaultValue(0)]
        public decimal TotalSoldVintagesValue { get; set; }

        /// <summary>
        /// Monthly virtual purchase of low risk portfolio shares
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal LowRiskShares { get; set; }

        /// <summary>
        /// Monthly virtual purchase of low risk portfolio shares
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal MediumRiskShares { get; set; }

        /// <summary>
        /// Monthly virtual purchase of low risk portfolio shares
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public decimal HighRiskShares { get; set; }

        /// <summary>
        /// The positive or negative difference compared to the last month.
        /// </summary>
        public decimal InvGainLoss { get; set; } = 0;

        #region Sold Info

        [Index("IDX_InvBalance_Cust_SoldYM_Sold", Order = 2)]
        [Index("IDX_InvBalance_Cust_YM_Sold", IsUnique = true, Order = 3)]
        public bool Sold { get; set; }

        /// <summary>
        /// Whether a bonus transaction has been awarded
        /// </summary>
        public bool AwardedSoldAmount { get; set; }

        /// <summary>
        /// On which month this vintage (month's shares) was sold
        /// </summary>
        [Index("IDX_InvBalance_Cust_SoldYM_Sold", Order = 3)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string SoldYearMonth { get; set; }

        /// <summary>
        /// Vintage Cash value when liquidated
        /// </summary>
        public decimal? SoldAmount { get; set; }

        /// <summary>
        /// Early withdrawal penalty when less than 90 days (3 month cycles) 25%.
        /// </summary>
        public decimal? EarlyCashoutFee { get; set; }
        
        /// <summary>
        /// Investment fee for greater than 10% vitnage gain (25% deduction).
        /// </summary>
        public decimal? HurdleFee { get; set; }
        
        /// <summary>
        /// Commission & fees by Gz, Funds
        /// </summary>
        public decimal? SoldFees { get; set; }
        public DateTime? SoldOnUtc { get; set; }
#endregion

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
        public InvBalance() {
            CashBonusAmount = CashInvestment = TotalCashInvestments = TotalSoldVintagesValue = 0M;
            PortfolioId = (int) RiskToleranceEnum.Medium;
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }
}