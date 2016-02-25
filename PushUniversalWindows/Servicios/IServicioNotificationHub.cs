using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Messaging;

namespace PushUniversalWindows.Servicios
{
    public interface IServicioNotificationHub
    {
        Task<Registration> Registrar();
    }
}