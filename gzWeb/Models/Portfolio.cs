﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public enum RiskToleranceEnum {

        Low = 1,

        Low_Medium,

        Medium,

        Medium_High,

        High
    }

    public class Portfolio {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public RiskToleranceEnum RiskTolerance { get; set; }

        public virtual ICollection<PortFund> PortFunds { get; set; }
    }
}