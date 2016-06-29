using System.Collections.Generic;
using gzDAL.Models;

namespace gzDAL.DTO {
    public class PortfolioDto {
        public int Id { get; set; }
        public string Title { get; set; }
        public double ROI { get; set; }
        public string Color { get; set; }
        public bool Selected { get; set; }
        public RiskToleranceEnum Risk { get; set; }
        public float AllocatedPercent { get; set; }
        public decimal AllocatedAmount { get; set; }
        public IEnumerable<HoldingDto> Holdings { get; set; }
    }

    public class HoldingDto {
        public string Name { get; set; }
        public double Weight { get; set; }
    }

}