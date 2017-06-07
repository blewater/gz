using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    [Table("PlayerRevRpt")]
    public class PlayerRevRpt {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("User ID")]
        [Index("IX_UserId_YM", IsUnique = true, Order = 1)]
        public int UserId { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Index("IX_UserId_YM", IsUnique = true, Order = 2)]
        [Index("IX_YM")]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(8)]
        public string YearMonthDay { get; set; }

        public decimal? BegGmBalance { get; set; }
        public decimal? EndGmBalance { get; set; }
        public decimal? GmGainLoss { get; set; }

        [Column("Player status")]
        [StringLength(256)]
        public string PlayerStatus { get; set; }

        [Column("Block reason")]
        [StringLength(256)]
        public string BlockReason { get; set; }

        [Column("Email address")]
        [Required]
        [StringLength(256)]
        public string EmailAddress { get; set; }

        [Column("Total deposits amount")]
        public decimal? TotalDepositsAmount { get; set; }

        public decimal? CashBonusAmount { get; set; }

        public decimal? Vendor2UserDeposits { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(30)]
        public string Mobile { get; set; }

        [StringLength(80)]
        public string City { get; set; }

        [StringLength(80)]
        public string Country { get; set; }

        [Column("PendingWithdrawals")]
        public decimal? PendingWithdrawals { get; set; }

        [Column("Withdraws made")]
        public decimal? WithdrawsMade { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(3)]
        public string Currency { get; set; }

        public int Processed { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public PlayerRevRpt() {
            Processed = 0;
            BegGmBalance = 0;
            EndGmBalance = 0;
            GmGainLoss = 0;
            TotalDepositsAmount = 0;
            WithdrawsMade = 0;
            PendingWithdrawals = 0;
            CashBonusAmount = 0;
            Vendor2UserDeposits = 0;
            CreatedOnUtc = DateTime.UtcNow;
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }
}
