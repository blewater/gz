using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// Greenzorro --> Casino Customer Bank account ? (requires special communication with casino as the intermediary
    /// 
    /// </summary>
    [Table("GzTrxs")]
    public class GzTrx {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Index("CustomerId_Mon_idx_gztransaction", IsUnique = false, Order = 1)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index("CustomerId_Mon_idx_gztransaction",IsUnique = false, Order = 2)]
        [Required, StringLength(6)]
        public string YearMonthCtd { get; set; }

        [Index("CustomerId_Mon_idx_gztransaction", IsUnique = false, Order = 3)]
        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public virtual GzTrxType Type { get; set; }

        /// <summary>
        /// 
        /// Reference Everymatrix Gm Transaction as source
        /// 
        /// </summary>
        [ForeignKey("GmTrx")]
        public int? GmTrxId { get; set; }
        public virtual GmTrx GmTrx { get; set; }

        /// <summary>
        /// 
        /// Reference self source
        /// http://stackoverflow.com/questions/4811194/what-is-the-syntax-for-self-referencing-foreign-keys-in-ef-code-first
        /// 
        /// </summary>
        public int? ParentTrxId { get; set; }
        [ForeignKey("ParentTrxId")]
        public virtual GzTrx ParentTrx { get; set; }

        // For Type:CreditedPlayingLoss -> the credit percentage for playing losses i.e. 50 for half
        public float? CreditPcntApplied { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }

        public decimal Amount { get; set; }
    }
}