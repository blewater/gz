using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// The type of business transaction journal entry.
    /// 
    /// </summary>
    public enum GzTransactionTypeEnum {

        /// <summary>
        /// 
        /// "Reserved for future use. Customer withdrawal from their greenzorro account 
        /// to their banking account." 
        /// Generate fees transactions: FundFee, Commission (4%)
        /// 
        /// </summary>
        InvWithdrawal = 1,

        /// <summary>
        /// 
        /// Sell portfolio shares and transfer cash to their casino account.
        /// 
        /// </summary>
        TransferToGaming = 2,

        /// <summary>
        /// 
        /// Liquidate all customer's funds to cash. 
        /// Typical Transaction when closing a customer's account.
        /// 
        /// </summary>
        FullCustomerFundsLiquidation = 3,

        /// <summary>
        /// 
        /// The %50 pcnt changes we credit to the players investment account.
        /// the whole amount "Playing Loss"
        /// <see cref="GmTransactionTypeEnum.PlayingLoss"/>
        /// 
        /// </summary>
        CreditedPlayingLoss = 4,

        /// <summary>
        /// 
        /// Fund fees: 2.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        FundFee = 5,

        /// <summary>
        /// 
        /// greenzorro fees: 1.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        GzFees = 6,

        /// <summary>
        /// 
        /// The realized greenzorro profit or loss by the actual purchase of customer shares 
        /// after the month's end.
        /// It is the total difference between 
        /// 
        /// "Bought Funds Prices" * "Monthly Customer Shares"
        ///         - 
        /// "Customer Shares" * "Funds Prices" credited to the customer's Account
        /// 
        /// </summary>
        GzActualTrxProfitOrLoss = 7,
    }

    /// <summary>
    /// 
    /// Captures a business greenzorro transaction log entry
    /// 
    /// </summary>
    public class GzTrxType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public GzTransactionTypeEnum Code { get; set; }

        [StringLength(300), Required]
        public string Description { get; set; }
    }
}