using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.ModelsUtil;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface IConfRepo {
        Task<GzConfiguration> GetConfRow();
    }
}