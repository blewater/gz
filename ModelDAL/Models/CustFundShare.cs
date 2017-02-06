using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzDAL.Models {
    /// <summary>
    /// Tracking new and existing balance of monthly shares per customer and fund
    /// </summary>
    public class CustFundShare {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 2)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 3)]
        public int FundId { get; set; }

        // Fund Nav Property
        [ForeignKey("FundId")]
        public virtual Fund Fund { get; set; }

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUtc { get; set; }

        #region Total Monthly Shares

        /// <summary>
        /// Total number of shares for month
        /// </summary>
        [Required]
        public decimal SharesNum { get; set; } = 0;

        /// <summary>
        /// $ Value of NumShares: Total number of shares for month.
        /// </summary>
        [Required]
        public decimal SharesValue { get; set; } = 0;

        /// <summary>
        /// Price of newly bought shares in month
        /// </summary>
        [ForeignKey("SharesFundPrice")]
        public int? SharesFundPriceId { get; set; }
        public virtual FundPrice SharesFundPrice { get; set; }

        /// <summary>
        /// Link to month's invBalance
        /// </summary>
        public int? InvBalanceId { get; set; }
        [ForeignKey("InvBalanceId")]
        public virtual InvBalance InvBalance { get; set; }

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

        #region Sold Vintage

        /// <summary>
        /// $ Value of NumShares: Total number of shares for month.
        /// </summary>
        public decimal? SoldSharesValue { get; set; }
        public int? SoldSharesFundPriceId { get; set; }
        public DateTime? SoldOnUtc { get; set; }
        #endregion

    }

    /// <summary>
    /// 
    /// Comparer for linq expressions
    /// 
    /// </summary>
    public class CustFundComparer : IEqualityComparer<CustFundShare> {
        public bool Equals(CustFundShare x, CustFundShare y) {
            return x.Id == y.Id;
        }
        public int GetHashCode(CustFundShare custFundShare) {
            return custFundShare.Id.GetHashCode();
        }
    }

}