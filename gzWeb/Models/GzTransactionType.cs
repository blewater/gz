using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    public enum TransferTypeEnum {

        Deposit = 1,

        Withdrawal = 2,

        TransferToGaming = 3,

        TransferToInvestment = 4,

        InvestmentRet = 5,

        PlayingLoss = 6, /* iGC gaming loss */

        /// <summary>
        /// In case the %50 pcnt changes we store the whole amount too "Playing Loss"
        /// </summary>
        CreditedPlayingLoss = 7, /* Playing loss * 50% */

        FundFee = 8, /* 1.5% */

        Commission = 9, /* 2.5% */

        Other = 10
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