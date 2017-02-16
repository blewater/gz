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
    public class VintageShares {

        #region Keys

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("UserPortfoliosShareId_YMD_idx", IsUnique = true, Order = 1)]
        public int UserId { get; set; }

        [Required]
        [Index("UserPortfoliosShareId_YMD_idx", IsUnique = true, Order = 2)]
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

        #region Portfolio Price Trade Days

        [Required]
        public DateTime BuyPortfolioTradeDay { get; set; }

        public DateTime? SoldPortfolioTradeDay { get; set; }

        #endregion

        /// <summary>
        /// Last Updated
        /// </summary>
        [Required]
        public DateTime UpdatedOnUtc { get; set; }
        public VintageShares() {
            PortfolioLowShares = 0;
            PortfolioMediumShares = 0;
            PortfolioHighShares = 0;
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }

}