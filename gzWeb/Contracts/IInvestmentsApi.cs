﻿using System.Collections.Generic;
using System.Web.Http;
using gzDAL.DTO;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Contracts
{
    public interface IInvestmentsApi
    {
        SummaryDataViewModel GetSummaryData(ApplicationUser user);

        IEnumerable<VintageViewModel> GetVintagesSellingValuesByUser(ApplicationUser user);

        IEnumerable<VintageDto> SaveDbSellVintages(int customerId, IEnumerable<VintageDto> vintages);
    }
}