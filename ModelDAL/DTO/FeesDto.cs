using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;

namespace gzDAL.DTO {
    public class FeesDto
    {
        public decimal GzFee { get; set; }
        public decimal FundFee { get; set; }
        public decimal EarlyCashoutFee { get; set; }
        public decimal HurdleFee { get; set; }
        public decimal Total { get; set; }
    }
}
