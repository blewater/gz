using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;

namespace gzDAL.DTO {
    public class VintageDto {
        public string YearMonthStr { get; set; }
        /// <summary>
        /// This month's <see cref="YearMonthStr"/> customers' cash amount that "invested" in 
        /// purchasing shares.
        /// </summary>
        public decimal InvestAmount { get; set; }
        /// <summary>
        /// The present market value without deducting Greenzorro or Fund fees
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// The <see cref="MarketPrice"/> after deducting Gz or Fund Fees
        /// </summary>
        public decimal SellingValue { get; set; }
        /// <summary>
        /// Gz and Fund fees. As of this writing in 4%
        /// </summary>
        public decimal Fees { get; set; }
        /// <summary>
        /// Have 6 months elapsed from investment so that this vintage is
        /// available for selling?
        /// </summary>
        public bool Locked { get; set; }
        /// <summary>
        /// Whether this vintage has been sold already.
        /// </summary>
        public bool Sold { get; set; }
        /// <summary>
        /// Whether the user selected this vintage to sell/liquidate to cash.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Shares comprising this vintage
        /// </summary>
        public IEnumerable<CustFundShareDto> CustomerVintageShares { get; set; } 
    }
}
