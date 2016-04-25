using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Contracts
{
    public interface IInvestmentsApi
    {
        SummaryDataViewModel GetSummaryData(ApplicationUser user);
    }
}