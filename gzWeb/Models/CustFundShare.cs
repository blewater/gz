using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    /// <summary>
    /// Tracking new and existing balance of monthly shares per customer and fund
    /// </summary>
    public class CustFundShare {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Non Unique: You may have multiple same fund share purchases
        /// within the month.
        /// <summary>
        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required, StringLength(6)]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 2)]
        public string YearMonth { get; set; }

        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 3)]
        public int FundId { get; set; }

        // Fund Nav Property
        [ForeignKey("FundId")]
        virtual public Fund Fund { get; set; }

        // Customer nav property
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUTC { get; set; }

        #region Total Monthly Shares
        /// <summary>
        /// Total number of shares for month
        /// </summary>
        [Required]
        public decimal SharesNum { get; set; }
        /// <summary>
        /// $ Value of NumShares: Total number of shares for month.
        /// </summary>
        [Required]
        public decimal SharesValue { get; set; }

        /// <summary>
        /// Price of newly bought shares in month
        /// </summary>
        [ForeignKey("SharesFundPrice")]
        public int? SharesFundPriceId { get; set; }
        public virtual FundPrice SharesFundPrice { get; set; }

        #endregion
        #region NewShares

        /// <summary>
        /// Number of new shares bought for month
        /// </summary>
        public decimal? NewSharesNum { get; set; }
        /// <summary>
        /// Value of new shares bought for month
        /// </summary>
        public decimal? NewSharesValue { get; set; }

        #endregion
    }
}