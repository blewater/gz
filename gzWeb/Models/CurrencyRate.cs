using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class CurrencyRate {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index("CurrRate_ftd_idx", IsUnique = true, Order = 1), Required]
        public DateTime TradeDateTime { get; set; }
        [StringLength(6), Index("CurrRate_ftd_idx", IsUnique = true, Order = 2), Required]
        public string FromTo { get; set; }

        [Required]
        public decimal rate { get; set; }

        public DateTime UpdatedOnUTC { get; set; }
    }
}