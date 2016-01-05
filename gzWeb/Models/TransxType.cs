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

        InvestmentRet,

        PlayingCost, /* 50% */

        FundFee, /* 1.5% */

        Other
    }

    public class TransxType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; }

        public TransferTypeEnum Reason { get; set; }

        [StringLength(128)]
        public string Description { get; set; }
    }
}