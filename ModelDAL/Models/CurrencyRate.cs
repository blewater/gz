using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// The exchange rate for a FromTO Currencies row.
    /// </summary>
    public class CurrencyRate {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index("CurrRate_ftd_idx", IsUnique = true, Order = 1), Required]
        public DateTime TradeDateTime { get; set; }
        [Index("CurrRate_ftd_idx", IsUnique = true, Order = 2), Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string FromTo { get; set; }

        [Required]
        public decimal rate { get; set; }

        public DateTime UpdatedOnUTC { get; set; }
    }
}