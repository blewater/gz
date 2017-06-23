using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// The last month that we imported the player month's beginning Balances
    /// </summary>
    public class PlayerRevLastMonth {

        /// <summary>
        /// Forced by Entity Framework to have a key 
        /// </summary>
        [Key]
        public int Id { get; set; } = 1;

        [Required]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonth { get; set; }

        [Required]
        public DateTime UpdatedOnUtc { get; set; }
    }
}