using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace gzWeb.Models {
    public class InvBalance {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key, Column(Order = 0)]
        public int CustId { get; set; }

        [ForeignKey("CustId")]
        public virtual ApplicationUser Customer { get; set; }

        public decimal Balance { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public int TransxId { get; set; }
        [ForeignKey("TransxId")]
        public virtual Transx Transx { get; set; }
    }
}