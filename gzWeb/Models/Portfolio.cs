﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class Portfolio {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(64)]
        public string RiskTolerance { get; set; }

        [StringLength(128)]
        public string Note { get; set; }

        public virtual ICollection<PortFund> PortFunds { get; set; }
    }
}