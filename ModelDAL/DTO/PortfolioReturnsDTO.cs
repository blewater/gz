using gzDAL.Models;

namespace gzDAL.DTO {
    public class PortfolioReturnsDTO {

        public int PortfolioId { get; set; }
        public float RoI { get; set; }
        public RiskToleranceEnum RiskEnumValue { get; set; }
        public bool IsActive { get; set; }

    }
}