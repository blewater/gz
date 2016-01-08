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

        Withdrawal,

        TransferToGaming,

        InvestmentRet,

        PlayingLoss, /* End of the month iGC loss */

        CreditedPlayingLoss, /* Playing loss * 50% */

        FundFee, /* 1.5% */

        Commission, /* 2.5% */

        Other
    }

    public class TransxType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public TransferTypeEnum Code { get; set; }

        [StringLength(128)]
        public string Description { get; set; }
    }
}