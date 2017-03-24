using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IUserPortfolioRepo {

        void SetDbUserMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC);
        Task<List<PortfolioDto>> GetUserPlansAsync(int userId);

        Task<Portfolio> GetCurrentCustomerPortfolio(int customerId);

        Task<Portfolio> GetPresentMonthsUserPortfolioAsync(int customerId);

        Task<Portfolio> GetUserPortfolioForThisMonthOrBeforeAsync(int customerId, string nextYearMonthStr);
        void SetDbDefaultPortfolio(int customerId, RiskToleranceEnum riskType);
        Task SetDbDefaultPorfolioAddGmUserId(int customerId, int gmUserId);
    }
}