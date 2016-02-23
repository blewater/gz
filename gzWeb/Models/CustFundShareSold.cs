using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    /// <summary>
    /// Tracking monthly sold shares
    /// Phase 1: this will be done for all the owned shares in a customer's 
    /// account.
    /// </summary>
    public class CustFundShareSold {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        #region Monthly Sold Shares
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

        [StringLength(128)]
        public string Rationale { get; set; }

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}