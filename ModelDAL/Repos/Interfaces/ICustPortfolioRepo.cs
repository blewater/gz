using gzDAL.Models;
using System;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustPortfolioRepo {

        void SaveDbCustomerSelectNextMonthsPortfolio(int customerId, RiskToleranceEnum riskType);
        void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
        void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
    }
}