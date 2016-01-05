using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Models {

    /// <summary>
    /// Transfers
    /// </summary>
    public class Transx {

        //User
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual TransxType Type { get; set; }

        public decimal Amount { get; set; }
    }
}