using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class CurrencyListX {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string From { get; set; }

        private string to = "USD";
        [Required]
        public string To {
            get { return to; }
            set { to = value; }
        }


        public DateTime UpdatedOnUTC { get; set; }
    }
}