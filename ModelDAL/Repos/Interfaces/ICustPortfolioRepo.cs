using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustPortfolioRepo {

        void SaveDbCustomerSelectNextMonthsPortfolio(int customerId, RiskToleranceEnum riskType);
        void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
        void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC);

        Task<List<PortfolioDto>> GetCustomerPlansAsync(int customerId);

        Task<Portfolio> GetCurrentCustomerPortfolio(int customerId);

        Task<Portfolio> GetNextMonthsCustomerPortfolioAsync(int customerId);

        Task<Portfolio> GetUserPortfolioForThisMonthOrBefore(int customerId, string nextYearMonthStr);
        Task SaveDefaultPorfolio(int customerId, int gmUserId);
    }
}