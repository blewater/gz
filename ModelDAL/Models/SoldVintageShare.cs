using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// Greenzorro --> Casino Customer Bank account ? (requires special communication with casino as the intermediary
    /// 
    /// </summary>
    public class SoldVintageShare {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SoldVintageId { get; set; }
        [ForeignKey("SoldVintageId")]
        public virtual SoldVintage SoldVintage { get; set; }

        [Required]
        [Index("CustFundShareId_YMD_idx", IsUnique = true, Order = 3)]
        public int FundId { get; set; }

        // Fund Nav Property
        [ForeignKey("FundId")]
        public virtual Fund Fund { get; set; }

        /// <summary>
        /// Total number of shares for month
        /// </summary>
        [Required]
        public decimal SharesNum { get; set; }

        /// TODO Ask Aki if sold vintages are liquidated any month day or
        /// at the end of the month
        /// if any month then we have to save their value here
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

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}