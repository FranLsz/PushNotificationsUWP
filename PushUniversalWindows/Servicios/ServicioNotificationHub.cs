using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.Messaging;
using PushUniversalWindows.Utiles;

namespace PushUniversalWindows.Servicios
{
    public class ServicioNotificationHub : IServicioNotificationHub
    {
        private readonly string _notificationHubPath;
        private readonly string _connectionString;
        private readonly IEnumerable<string> _tags;

        public ServicioNotificationHub(string notificationHubPath, string connectionString, IEnumerable<string> tags = null)
        {
            _notificationHubPath = notificationHubPath;
            _connectionString = connectionString;
            _tags = tags;
        }

        public async Task<Registration> Registrar()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            var hub = new NotificationHub(_notificationHubPath, _connectionString);
            return await hub.RegisterNativeAsync(channel.Uri, _tags);
        }
    }
}