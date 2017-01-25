using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzDAL.Models {
    /// <summary>
    /// Vintage optimized customer monthly portfolio shares tracking 
    /// </summary>
    public class CustPortfoliosShares {

        #region Keys

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustPortfoliosShareId_YMD_idx", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }

        [Required]
        [Index("CustPortfoliosShareId_YMD_idx", IsUnique = true, Order = 2)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        #endregion

        #region Portfolios Shares

        [Required]
        [DefaultValue(0)]
        public decimal PortfolioLowShares { get; set; } = 0;

        [Required]
        [DefaultValue(0)]
        public decimal PortfolioMediumShares { get; set; } = 0;

        [Required]
        [DefaultValue(0)]
        public decimal PortfolioHighShares { get; set; } = 0;

        #endregion

        /// <summary>
        /// Just Track the monthly price of the portfolio
        /// </summary>

        #region Portfolio Prices

        [Required]
        public float BuyPortfolioLowPrice { get; set; } = 0;

        [Required]
        public float BuyPortfolioMediumPrice { get; set; } = 0;

        [Required]
        public float BuyPortfolioHighPrice { get; set; } = 0;

        #endregion

        /// <summary>
        ///  Track the specific month's trading day used to calculate cash value
        /// </summary>

        #region Sold Portfolio Prices

        //[ForeignKey("PortfolioPrices")]
        public int? SoldPortfolioLowPriceId { get; set; }
        public virtual PortfolioPrice SoldPortfolioLowPrice { get; set; }


        //[ForeignKey("PortfolioPrices")]
        public int? SoldPortfolioMediumPriceId { get; set; }
        public virtual PortfolioPrice SoldPortfolioMediumPrice { get; set; }

        //[ForeignKey("PortfolioPrices")]
        public int? SoldPortfolioHighPriceId { get; set; }
        public virtual PortfolioPrice SoldPortfolioHighPrice { get; set; }

        #endregion

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUtc { get; set; }
        public CustPortfoliosShares() {

            PortfolioLowShares = 0;
            PortfolioMediumShares = 0;
            PortfolioHighShares = 0;
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }

}