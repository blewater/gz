using System;

namespace gzDAL.DTO {
    /// <summary>
    /// Tracking portfolio shares along with their worth
    /// </summary>
    public class VintageSharesDto {
        public decimal LowRiskShares;
        public decimal MediumRiskShares;
        public decimal HighRiskShares;
        public decimal PresentMarketPrice { get; set; }
        public DateTime TradingDay { get; set; }
    }
}