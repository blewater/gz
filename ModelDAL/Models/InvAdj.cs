using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    public enum AmountTypeEnum {

        Deposit = 1,

        Withdrawal = 2
        
    }

    /// <summary>
    /// 
    /// Manual monthly amount adjustments for investment balances.
    /// 
    /// </summary>
    public class InvAdj {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index("Inv_Adjustment_Idx", IsUnique = true, Order = 1)]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [Index("Inv_Adjustment_Idx", IsUnique = true, Order = 2)]
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public AmountTypeEnum AmountType { get; set; }

        /// <summary>
        /// 
        /// (+ / -) Adjustment.
        /// 
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
        public InvAdj() {
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }
}