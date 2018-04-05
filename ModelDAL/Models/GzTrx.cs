using System;
using System.ComponentModel;
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

        // Used for the last investment sql function query
        [Index("IX_CustomerId_YM_TId_Amnt", Order = 1)]
        [Index("IX_CustomerId_TId_Amnt", Order = 1)]
        [Index("IX_CustomerId_YM_TTyp", Order = 1)]
        [Required]
        public int CustomerId { get; set; }

        // Used for the last investment sql function
        [Index("IX_CustomerId_YM_TId_Amnt", Order = 2)]
        [Index("IX_CustomerId_YM_TTyp", Order = 2)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonthCtd { get; set; }

        // Used for the last investment sql function
        [Index("IX_CustomerId_YM_TId_Amnt", Order = 3)]
        [Index("IX_CustomerId_YM_TTyp", Order = 3)]
        [Index("IX_CustomerId_TId_Amnt", Order = 2)]
        [ForeignKey("Type")]
        public int TypeId { get; set; }

        // Used for the last investment sql function
        [Index("IX_CustomerId_YM_TId_Amnt", Order = 4)]
        [Index("IX_CustomerId_TId_Amnt", IsUnique = false, Order = 3)]
        public decimal Amount { get; set; }
        /// <summary>
        /// Monthly Starting Gaming balance imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        public decimal? BegGmBalance { get; set; }

        /// <summary>
        /// Monthly deposits imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        public decimal? Deposits { get; set; }

        /// <summary>
        /// Monthly withdrawals imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        public decimal? Withdrawals { get; set; }

        /// <summary>
        /// Monthly Ending Gaming balance imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        public decimal? EndGmBalance { get; set; }

        /// <summary>
        /// Monthly Ending Gaming balance imported from Everymatrix reports
        /// </summary>
        [DefaultValue(0)]
        public decimal? GmGainLoss { get; set; }

        [DefaultValue(0)]
        [Required]
        public decimal CashBonusAmount { get; set; }

        [DefaultValue(0)]
        [Required]
        public decimal CashDeposits { get; set; }

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

        public DateTime? JoinDate { get; set; }

        public DateTime? FirstDeposit { get; set; }

        public DateTime? LastDeposit { get; set; }

        public DateTime? LastPlayedDate { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }
        #region Foreign Entities

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        public virtual GzTrxType Type { get; set; }

        #endregion
    }
}