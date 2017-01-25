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

        [Index("IDX_PortfolioPrice_Id_YMD", IsUnique = true, Order = 1)]
        [ForeignKey("Portfolio")]
        [Required]
        public int PortfolioId { get; set; }
        public virtual Portfolio Portfolio { get; set; }

        /// <summary>
        /// The monthly cleared typically based on last month's trading 
        /// funds closing prices.
        /// </summary>
        [Index("IDX_PortfolioPrice_Id_YMD", IsUnique = true, Order = 2)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(8)]
        public string YearMonthDay { get; set; }

        /// <summary>
        /// Its value in virtual currency though in $ practically it will
        /// be used to track portfolio value appreciation
        /// </summary>
        [Required]
        public float Price { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}