using gzWeb.Models;

namespace gzWeb.Repo.Interfaces
{
    public interface ICustomerRepo
    {
        int CreateOrUpdateUser(ApplicationUser newUser, string password);
        
    }
}