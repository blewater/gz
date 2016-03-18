using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.Models {

    /// <summary>
    /// Transfers between Casino <--> Greenzorro accounts, Casino <--> Customer bank account, 
    /// Greenzorro <--> Customer Bank account ? (requires special communication with casino as the intermediatery
    /// </summary>
    public class GzTransaction {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Index("CustomerId_Mon_idx_gztransaction", IsUnique = false, Order = 1)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index("CustomerId_Mon_idx_gztransaction",IsUnique = false, Order = 2)]
        [Required, StringLength(6)]
        public string YearMonthCtd { get; set; }

        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public virtual GzTransactionType Type { get; set; }

        // For Type:CreditedPlayingLoss -> the credit percentage for playing losses i.e. 50 for half
        public float? CreditPcntApplied { get; set; }

        public int? CurrencyRateId { get; set; }
        [ForeignKey("CurrencyRateId")]
        public virtual CurrencyRate CurrencyRateToUSD { get; set; }

        [Index, Required]
        public DateTime CreatedOnUTC { get; set; }

        public decimal Amount { get; set; }
    }
}