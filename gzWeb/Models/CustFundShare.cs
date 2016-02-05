using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    public class CustFundShare {
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

        /// <summary>
        /// Total number of shares for month
        /// </summary>
        [Required]
        public decimal NumShares { get; set; }
        /// <summary>
        /// $ Value of NumShares
        /// </summary>
        [Required]
        public decimal Value { get; set; }

        /// <summary>
        /// Number of new shares bought for month
        /// </summary>
        [Required]
        public decimal NewNumShares { get; set; }

        /// <summary>
        /// Price of newly bought shares in month
        /// </summary>
        [ForeignKey("BoughtFundPrice")]
        public int? BoughtFundPriceId { get; set; }
        public virtual FundPrice BoughtFundPrice { get; set; }
        public DateTime? TradeDayofNewSharesUTC { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}