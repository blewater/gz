using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gzDAL.DTO {
    public class VintageDto {
        public string YearMonthStr { get; set; }
        public decimal InvestAmount { get; set; }
        public decimal SellingValue { get; set; }
        public bool SellThisMonth { get; set; }
    }
}
