using System.Threading.Tasks;

namespace gzWeb.Utilities {
    public interface ICacheUserData {
        Task Query(int userId);
    }
}