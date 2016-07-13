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

        [Index("IDX_FundPrice_Id_YMD", IsUnique = true, Order = 1)]
        [Required]
        public int FundId { get; set; }

        /// <summary>
        /// The completed after 4pm trade day that we use the closing price.
        /// </summary>
        [Index("IDX_FundPrice_Id_YMD", IsUnique = true, Order = 2)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(8)]
        public string YearMonthDay { get; set; }

        [Required]
        public float ClosingPrice { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}