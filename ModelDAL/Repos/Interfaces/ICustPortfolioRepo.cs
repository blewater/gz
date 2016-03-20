using gzDAL.Models;
using System;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustPortfolioRepo
    {
        void SaveDBCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
        void SaveDBCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
    }
}