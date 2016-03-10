using gzWeb.Models;

namespace gzWeb.Repo.Interfaces
{
    public interface ICustomerRepo
    {
        int CreateUpdUser(CustomerDTO dto);
    }
}