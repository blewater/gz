using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// Tracking monthly price of a virtual Greenzorro portfolio share 
    /// after the calculation of its portfolio shares by tracking 
    /// its funds closing prices on a monthly last trading day basis.
    /// </summary>
    public class PortfolioPrice {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The monthly cleared typically based on last month's trading 
        /// funds closing prices.
        /// </summary>
        [Index("IDX_PortfolioPrices_YMD", IsUnique = true)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(8)]
        public string YearMonthDay { get; set; }

        /// <summary>
        /// Its value in $ though it will
        /// be used to track portfolio value appreciation in any user denominated currency
        /// </summary>
        [Required]
        public double PortfolioLowPrice { get; set; }

        [Required]
        public double PortfolioMediumPrice { get; set; }

        [Required]
        public double PortfolioHighPrice { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}