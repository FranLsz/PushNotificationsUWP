using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Messaging;
using Microsoft.WindowsAzure.MobileServices;
using PushUniversalWindows.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PushUniversalWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
             MobileServiceClient MobileService = new MobileServiceClient(
                  "https://franpush.azure-mobile.net/",
                  "EfJxzclqkyQUQqdanbivlLhkqXJPqX65"
            );

            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            var hub = new NotificationHub("franpushhub", "Endpoint=sb://franpushhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=dGXPuPziKj5jyyTKL88BxCe4G0lqWxqnUsIe+/01Ijk=");

            var result = await hub.RegisterNativeAsync(channel.Uri);

            TodoItem item = new TodoItem { Text = "Awesome item", Complete = false };
            await MobileService.GetTable<TodoItem>().InsertAsync(item);

            var dialog = new Windows.UI.Popups.MessageDialog(item.Id);
            await dialog.ShowAsync();
        }
    }
}
