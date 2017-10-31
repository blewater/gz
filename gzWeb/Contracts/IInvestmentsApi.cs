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

        Task<List<VintageViewModel>> GetVintagesSellingValuesByUserTestHelper(ApplicationUser user);

        ICollection<VintageDto> SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages, bool sendEmail2Admins, string emailAdmins, string gmailUser, string gmailPwd);

        Task<List<PlanViewModel>> GetCustomerPlansAsync(int userId, decimal nextInvestAmount = 0);
    }
}