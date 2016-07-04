using System;
using System.Collections.Generic;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelsUtil;

namespace gzDAL.Repos.Interfaces
{
    public interface IUserRepo {
        UserSummaryDTO GetSummaryData(int userId, out ApplicationUser user);
    }
}