using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    public class InvBalance {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique=true, Order=1)]
        public int CustomerId { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_invbal", IsUnique = true, Order=2)]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        /// <summary>
        /// Investment Cash it will be zero in most cases.
        /// </summary>
        [Required]
        public decimal Balance { get; set; }

        /// <summary>
        /// The positive or negative difference compared to the last month.
        /// </summary>
        private decimal invGainLoss = 0;
        public decimal InvGainLoss {
            get { return invGainLoss; }
            set { invGainLoss = value; }
        }

        /// <summary>
        /// The month's cash investment
        /// </summary>
        private decimal cashInvestment = 0;
        public decimal CashInvestment {
            get { return cashInvestment; }
            set { cashInvestment = value; }
        }

        /// <summary>
        /// The conversion used for the month's cash investment if player's currency is different than $USD
        /// </summary>
        public int? CashConvRateId { get; set; }
        [ForeignKey("CashConvRateId")]
        public virtual CurrencyRate CashConvRate { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}