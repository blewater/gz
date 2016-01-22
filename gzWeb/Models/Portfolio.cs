using System;
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

    /// A collection of funds...weighted (from Conservartive to High Stakes).
    /// Many to Many with Funds with additional fields in association table PortFund
    /// Design http://stackoverflow.com/questions/7050404/create-code-first-many-to-many-with-additional-fields-in-association-table
    public class Portfolio {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique=true), Required]
        public RiskToleranceEnum RiskTolerance { get; set; }

        public virtual ICollection<PortFund> PortFunds { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Based on max(3yr, 5yr) returns of the individual funds what's the weighted funds portfolio return
        /// </summary>
        [NotMapped]
        public float AvgReturn {
            get {
                return this.PortFunds.Select(f => f.Weight).Average();
            }
        }
    }
}