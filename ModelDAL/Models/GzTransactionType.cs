using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.Models {

    /// <summary>
    /// 
    /// The type of business transaction journal entry.
    /// 
    /// </summary>
    public enum GzTransactionJournalTypeEnum {

        /// <summary>
        /// 
        /// Customer deposit to their casino account. 
        /// Informational purpose only.
        /// 
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// 
        /// "Reserved for future use. Customer withdrawal from their Greenzorro account 
        /// to their banking account." 
        /// Generate fees transactions: FundFee, Commission (4%)
        /// 
        /// </summary>
        InvWithdrawal = 2,

        /// <summary>
        /// 
        /// Sell portfolio shares and transfer cash to their casino account.
        /// 
        /// </summary>
        TransferToGaming = 3,

        /// <summary>
        /// 
        /// Player cash withdrawal from their casino account. 
        /// Informational purpose only.
        /// 
        /// </summary>
        CasinoWithdrawal = 4,

        /// <summary>
        /// 
        /// Liquidate all customer's funds to cash. 
        /// Typical Transaction when closing a customer's account.
        /// 
        /// </summary>
        FullCustomerFundsLiquidation = 5,

        /// <summary>
        /// 
        /// Customer Casino loss.
        /// We credit a percentage i.e. 50% from this amount.
        /// <see cref="CreditedPlayingLoss"/>
        /// 
        /// </summary>
        PlayingLoss = 6,

        /// <summary>
        /// 
        /// The %50 pcnt changes we credit to the players investment account.
        /// the whole amount "Playing Loss"
        /// <see cref="PlayingLoss"/>
        /// 
        /// </summary>
        CreditedPlayingLoss = 7,

        /// <summary>
        /// 
        /// Fund fees: 2.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        FundFee = 8,

        /// <summary>
        /// 
        /// Greenzorro fees: 1.5%
        /// Deducted from the customer investment when withdrawing cash.
        /// 
        /// </summary>
        GzFees = 9,

        /// <summary>
        /// 
        /// The realized Greenzorro profit or loss by the actual purchase of customer shares 
        /// after the month's end.
        /// It is the total difference between 
        /// 
        /// "Bought Funds Prices" * "Monthly Customer Shares"
        ///         - 
        /// "Customer Shares" * "Funds Prices" credited to the customer's Account
        /// 
        /// </summary>
        GzActualTrxProfitOrLoss = 10
    }

    /// <summary>
    /// 
    /// Captures a business Greenzorro transaction log entry
    /// 
    /// </summary>
    public class GzTransactionType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public GzTransactionJournalTypeEnum Code { get; set; }

        [StringLength(300), Required]
        public string Description { get; set; }

        public virtual ICollection<GzTransaction> GzTransactions { get; set; }
    }
}