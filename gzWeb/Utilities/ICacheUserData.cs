using System.Threading.Tasks;

namespace gzWeb.Utilities {
    public interface ICacheUserData {
        void Query(int userId);
    }
}