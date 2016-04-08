using System;

namespace gzDAL.DTO {

    /// <summary>
    /// 
    /// Used in Funds biz handling methods to return funds values from the calculating methods.
    /// 
    /// </summary>
    public class PortfolioFundDTO {

        public int? PortfolioId { get; set; }
        public int FundId { get; set; }
        public float Weight { get; set; }
        public decimal SharesNum { get; set; }
        public decimal SharesValue { get; set; }
        public string SharesTradeDay { get; set; }
        public int SharesFundPriceId { get; set; }

        // Additional positive or negative shares
        public decimal NewSharesNum { get; set; }
        public decimal NewSharesValue { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
    }
}