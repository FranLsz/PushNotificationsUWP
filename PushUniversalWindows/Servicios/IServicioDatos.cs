using System.Collections.Generic;
using System.Threading.Tasks;
using PushUniversalWindows.Model;

namespace PushUniversalWindows.Servicios
{
    public interface IServicioDatos
    {
        Task<List<Smartphone>> GetSmartphones();
        Task<Smartphone> AddSmartphone(Smartphone model);
        void DeleteSmartphone(Smartphone model);
    }
}