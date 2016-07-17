using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzDAL.Models {

    /// <summary>
    /// 
    /// Transfers between Casino <--> greenzorro accounts, Casino <--> Customer bank account
    /// 
    /// </summary>
    [Table("GmTrxs")]
    public class GmTrx {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [Index]
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string YearMonthCtd { get; set; }

        [Index]
        public int? GmCustomerId { get; set; }

        [Index]
        [StringLength(256)]
        public string CustomerEmail { get; set; }   

        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public virtual GmTrxType Type { get; set; }

        [Required]
        public DateTime CreatedOnUtc { get; set; }

        public decimal Amount { get; set; }
    }
}