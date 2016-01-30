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

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        public decimal Balance { get; set; }

        [Index("CustomerId_Mon_idx_invbal", IsUnique=true, Order=1)]
        public int CustomerId { get; set; }

        [Index("CustomerId_Mon_idx_invbal", IsUnique = true, Order=2)]
        [StringLength(6)]
        public string YearMonthCtd { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}