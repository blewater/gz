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
    public enum GmTransactionTypeEnum {

        /// <summary>
        /// 
        /// Customer deposit to their casino account. 
        /// Informational purpose only.
        /// 
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// 
        /// Player cash withdrawal from their casino account. 
        /// Informational purpose only.
        /// 
        /// </summary>
        CasinoWithdrawal = 2,

        /// <summary>
        /// 
        /// Customer Casino loss.
        /// We credit a percentage i.e. 50% from this amount.
        /// <see cref="GzTransactionTypeEnum.CreditedPlayingLoss"/>
        /// 
        /// </summary>
        PlayingLoss = 3,

        /// <summary>
        /// 
        /// N/A or not initially useful to the greenzorro business logic
        /// 
        /// </summary>
        Other = 4
    }

    /// <summary>
    /// 
    /// Captures a business greenzorro transaction log entry
    /// 
    /// </summary>
    public class GmTrxType {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public GmTransactionTypeEnum Code { get; set; }

        [StringLength(300), Required]
        public string Description { get; set; }
    }
}