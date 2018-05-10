using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {
    public class Betting {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(8)]
        public string YearMonthDay { get; set; }

        public float Rounds { get; set; }
        public float UserWin { get; set; }
        public float Payout { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public Betting() {
            Rounds = UserWin = Payout = 0;
            CreatedOnUtc = DateTime.UtcNow;
        }
    }
}
