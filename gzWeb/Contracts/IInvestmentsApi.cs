using System.Collections.Generic;
using System.Web.Http;
using gzDAL.DTO;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Contracts
{
    public interface IInvestmentsApi
    {
        SummaryDataViewModel GetSummaryData(ApplicationUser user, UserSummaryDTO summaryDto);

        IEnumerable<VintageViewModel> GetVintagesSellingValuesByUser(ApplicationUser user);

        ICollection<VintageDto> SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages);

        IEnumerable<PlanViewModel> GetCustomerPlans(int customerId, decimal nextInvestAmount = 0);
    }
}