using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using gzDAL.DTO;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Contracts
{
    public interface IInvestmentsApi
    {
        SummaryDataViewModel GetSummaryData(ApplicationUser user, UserSummaryDTO summaryDto);

        List<VintageViewModel> GetVintagesSellingValuesByUserTestHelper(ApplicationUser user);

        List<PlanViewModel> GetCustomerPlans(int userId, decimal nextInvestAmount = 0);
    }
}