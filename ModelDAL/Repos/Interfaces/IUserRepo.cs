using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelsUtil;

namespace gzDAL.Repos.Interfaces
{
    public interface IUserRepo {

        ApplicationUser GetCachedUser(int userId);
        UserSummaryDTO GetSummaryData(int userId, out ApplicationUser userRet);

    }
}