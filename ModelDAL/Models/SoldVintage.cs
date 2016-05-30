using System;
using System.Collections.Generic;
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
        public string VintageYearMonth { get; set; }

        [Required]
        public decimal MarketAmount { get; set; }

        [Required]
        public decimal Fees { get; set; }

        public virtual ICollection<SoldVintageShare> SoldVintageShares { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}