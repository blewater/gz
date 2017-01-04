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
        public decimal GamingGainLoss { get; set; }

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

        /// <summary>
        /// The month's cash investment
        /// </summary>
        [Index("IDX_InvBalance_Cust_YM_CashInv", IsUnique = true, Order = 3)]
        public decimal CashInvestment { get; set; } = 0;

        /// <summary>
        /// Optional Cash balance for when the portfolio is sold.
        /// </summary>
        public decimal? CashBalance { get; set; } = 0;

        /// <summary>
        /// The positive or negative difference compared to the last month.
        /// </summary>
        public decimal InvGainLoss { get; set; } = 0;

        #region Sold Info


        [Index("IDX_InvBalance_Cust_SoldYM_Sold", Order = 2)]
        [Index("IDX_InvBalance_Cust_YM_Sold", IsUnique = true, Order = 3)]
        public bool Sold { get; set; }

        [Index("IDX_InvBalance_Cust_SoldYM_Sold", Order = 3)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string SoldYearMonth { get; set; }
        public decimal? SoldAmount { get; set; }
        public decimal? SoldFees { get; set; }
        public DateTime? SoldOnUtc { get; set; }
        public virtual ICollection<CustFundShare> SoldShares { get; set; }
#endregion

        [Required]
        public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
    }
}