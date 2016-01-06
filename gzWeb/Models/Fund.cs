﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class Fund {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique = true), MaxLength(10)]
        public string Symbol { get; set; }

        [Index(IsUnique = true), Required, MaxLength(128)]
        public string HoldingName { get; set; }
    }
}