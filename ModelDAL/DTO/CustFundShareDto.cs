using System;

namespace gzDAL.DTO {
    /// <summary>
    /// Tracking new and existing balance of monthly shares per customer and fund
    /// </summary>
    public class CustFundShareDto {

        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string YearMonth { get; set; }

        public int FundId { get; set; }

        #region Total Monthly Shares

        /// <summary>
        /// Total number of shares for month
        /// </summary>
        public decimal SharesNum { get; set; }
        /// <summary>
        /// $ Value of NumShares: Total number of shares for month.
        /// </summary>
        public decimal SharesValue { get; set; }

        #endregion

        public int? SharesFundPriceId { get; set; }

        public int? SoldInvBalanceId { get; set; }

        #region NewShares

        /// <summary>
        /// Number of new shares bought for month
        /// </summary>
        public decimal? NewSharesNum { get; set; }
        /// <summary>
        /// Value of new shares bought for month
        /// </summary>
        public decimal? NewSharesValue { get; set; }

        #endregion

        public DateTime UpdatedOnUtc { get; set; }
    }
}