using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {

    /// <summary>
    /// Transfers
    /// </summary>
    public class GzTransaction {

        //User
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Index("CustomerId_Mon_idx_gztransaction", 1, IsUnique = false)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index("CustomerId_Mon_idx_gztransaction", 2, IsUnique = false)]
        [Required, MinLength(6), StringLength(6)]
        public string YearMonthCtd { get; set; }

        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public virtual GzTransactionType Type { get; set; }

        [Index, Required]
        public DateTime CreatedOnUTC { get; set; }

        public decimal Amount { get; set; }
    }
}