using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.Models {
    public enum TransferTypeEnum {

        /// <summary>
        /// Player deposit to their casino account. 
        /// Informational only
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// Player withdrawal from their investment account.
        /// Generate fees transactions: FundFee, Commission (4%)
        /// </summary>
        InvWithdrawal = 2,

        /// <summary>
        /// Sell portfolio shares and transfer cash to their casino account.
        /// </summary>
        TransferToGaming = 3,

        /// <summary>
        /// Net player withdrawal from their casino account. 
        /// Informational only
        /// </summary>
        CasinoWithdrawal = 4,

        /// <summary>
        /// Total Casino losses.
        /// We credit a percentage i.e. 50% from this amount.
        /// <see cref="CreditedPlayingLoss"/>
        /// </summary>
        PlayingLoss = 6,

        /// <summary>
        /// The %50 pcnt changes we credit to the players investment account.
        /// the whole amount "Playing Loss"
        /// <see cref="PlayingLoss"/>
        /// </summary>
        CreditedPlayingLoss = 7,

        /// <summary>
        /// Fund fees: 2.5%
        /// Deducted from the investment cash when withdrawn cash
        /// </summary>
        FundFee = 8,

        /// <summary>
        /// Greenzorro fees: 1.5%
        /// Deducted from the investment cash when withdrawn cash
        /// </summary>
        GzFees = 9,
    }

    public class GzTransactionType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public TransferTypeEnum Code { get; set; }

        [StringLength(128)]
        public string Description { get; set; }

        public virtual ICollection<GzTransaction> GzTransactions { get; set; }
    }
}