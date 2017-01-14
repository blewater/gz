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

        public decimal? BegBalance { get; set; }
        public decimal? EndBalance { get; set; }
        public decimal? PlayerLoss { get; set; }

        [StringLength(256)]
        public string Role { get; set; }

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

        [Column("Last login")]
        public DateTime? LastLogin { get; set; }

        [Column("Accepts bonuses")]
        public bool? AcceptsBonuses { get; set; }

        [Column("Total deposits amount")]
        public decimal? TotalDepositsAmount { get; set; }

        [Column("Withdraws made")]
        public decimal? WithdrawsMade { get; set; }

        [Column("Last played date", TypeName = "date")]
        public DateTime? LastPlayedDate { get; set; }

        [Column("Deposits made")]
        public decimal? DepositsMade { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(3)]
        public string Currency { get; set; }

        [Column("Total bonuses accepted by the player")]
        public decimal? TotalBonusesAcceptedByThePlayer { get; set; }

        [Column("Net revenue")]
        public decimal? NetRevenue { get; set; }

        [Column("Gross revenue")]
        public decimal? GrossRevenue { get; set; }

        public decimal? NetLossInUSD { get; set; }

        [Column("Real money balance")]
        public decimal? RealMoneyBalance { get; set; }

        public int Processed { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public PlayerRevRpt() {
            Processed = 0;
            CreatedOnUtc = DateTime.UtcNow;
            UpdatedOnUtc = DateTime.UtcNow;
        }
    }
}
