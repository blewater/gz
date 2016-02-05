using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {
    public class FundPrice {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("FundId")]
        virtual public Fund Fund { get; set; }

        [Required]
        [Index("FundId_YMD_idx", IsUnique = true, Order = 1)]
        public int FundId { get; set; }

        [StringLength(8)]
        [Index("FundId_YMD_idx", IsUnique = true, Order = 2)]
        public string YearMonthDay { get; set; }

        [Required]
        public float ClosingPrice { get; set; }

        // Tracking the price only for the shares bought
        [ForeignKey("CustNewFundShare")]
        public int? CustNewFundShareId { get; set; }
        virtual public CustFundShare CustNewFundShare { get; set; }

        [Required]
        public DateTime UpdatedOnUTC { get; set; }
    }
}