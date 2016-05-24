using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// Greenzorro --> Casino Customer Bank account ? (requires special communication with casino as the intermediary
    /// 
    /// </summary>
    public class SoldVintage {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Index("CustomerId_Mon_idx_gzSoldVintage", IsUnique = false, Order = 1)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index("CustomerId_Mon_idx_gzSoldVintage",IsUnique = false, Order = 2)]
        [Required, StringLength(6)]
        public string YearMonthCtd { get; set; }

        /// <summary>
        /// 
        /// Reference Everymatrix Gm Transaction as source
        /// 
        /// </summary>
        [ForeignKey("GzTrx")]
        public int? GzTrxId { get; set; }
        public virtual GzTrx GzTrx { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }

        public decimal Amount { get; set; }
    }
}