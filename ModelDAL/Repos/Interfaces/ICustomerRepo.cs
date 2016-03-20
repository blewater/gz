using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustomerRepo
    {
        int CreateOrUpdateUser(ApplicationUser newUser, string password);
        
    }
}