using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// greenzorro --> Casino Customer Bank account ? (requires special communication with casino as the intermediary
    /// 
    /// </summary>
    public class SoldVintage {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("CustomerId_Mon_idx_gzSoldVintage", IsUnique = true, Order = 1)]
        [Index("IX_Sold_Vintages_Cust_VintageYearMonth", IsUnique = true, Order = 1)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index("CustomerId_Mon_idx_gzSoldVintage", IsUnique = true, Order = 2)]
        [Index("IX_Sold_Vintages_Cust_VintageYearMonth", IsUnique = true, Order = 2)]
        [Index("IX_Sold_Vintages_VintageYearMonth")]
        [Column(TypeName = "char")]
        [StringLength(6)]
        [Required]
        public string VintageYearMonth { get; set; }

        [Index("CustomerId_Mon_idx_gzSoldVintage",IsUnique = true, Order = 3)]
        [Column(TypeName = "char")]
        [StringLength(6)]
        [Required]
        public string YearMonth { get; set; }

        [Required]
        public decimal MarketAmount { get; set; }

        [Required]
        public decimal Fees { get; set; }

        public virtual ICollection<CustFundShare> VintageShares { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}