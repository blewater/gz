using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    /// <summary>
    /// 
    /// greenzorro --> Casino Customer Bank account ? (requires special communication with casino as the intermediary
    /// 
    /// </summary>
    [Table("GzTrxs")]
    public class GzTrx {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index("IX_CustomerId_YM_TId_Amnt", IsUnique = true, Order = 1)]
        [Index("IX_CustomerId_TId_Amnt", IsUnique = false, Order = 1)]
        [Index("IX_CustomerId_YM_TTyp", IsUnique = true, Order = 1)]
        [Required]
        public int CustomerId { get; set; }

        [Index("IX_CustomerId_YM_TId_Amnt", IsUnique = true, Order = 2)]
        [Index("IX_CustomerId_YM_TTyp", IsUnique = true, Order = 2)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonthCtd { get; set; }

        [Index("IX_CustomerId_YM_TId_Amnt", IsUnique = true, Order = 3)]
        [Index("IX_CustomerId_YM_TTyp", IsUnique = true, Order = 3)]
        [Index("IX_CustomerId_TId_Amnt", IsUnique = false, Order = 2)]
        [ForeignKey("Type")]
        public int TypeId { get; set; }

        [Index("IX_CustomerId_YM_TId_Amnt", IsUnique = true, Order = 4)]
        [Index("IX_CustomerId_TId_Amnt", IsUnique = false, Order = 3)]
        public decimal Amount { get; set; }

        /// <summary>
        /// 
        /// Reference PlayerRevRpt as the source of player balance amounts
        /// 
        /// </summary>
        [ForeignKey("PlayerRevRpt")]
        public int? PlayerRevRptId { get; set; }
        public virtual PlayerRevRpt PlayerRevRpt { get; set; }

        /// <summary>
        /// 
        /// For Type:CreditedPlayingLoss -> the credit percentage for playing losses i.e. 50 for half
        /// 
        /// </summary>
        public float? CreditPcntApplied { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }
        #region Foreign Entities

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        public virtual GzTrxType Type { get; set; }

        #endregion
    }
}