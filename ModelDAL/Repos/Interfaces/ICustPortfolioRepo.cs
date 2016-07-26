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

        Task<IEnumerable<PortfolioDto>> GetCustomerPlansAsync(int customerId);

        Portfolio GetCurrentCustomerPortfolio(int customerId);

        Portfolio GetNextMonthsCustomerPortfolio(int customerId);

        Portfolio GetCustomerPortfolioForMonth(int customerId, string yearMonthStr);
        void SaveDefaultPorfolio(int customerId, int gmUserId);
    }
}