using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// The stock market closing price after the completion of the trading day.
    /// </summary>
    public class FundPrice {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("FundId")]
        virtual public Fund Fund { get; set; }

        [Required]
        [Index("FundId_YMD_idx", IsUnique = true, Order = 1)]
        public int FundId { get; set; }

        /// <summary>
        /// The completed after 4pm trade day that we use the closing price.
        /// </summary>
        [StringLength(8)]
        [Index("FundId_YMD_idx", IsUnique = true, Order = 2)]
        public string YearMonthDay { get; set; }

        [Required]
        public float ClosingPrice { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}