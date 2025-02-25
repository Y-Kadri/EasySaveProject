using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Server
{
    public static class Connexion
    {
        private static string serverIp = "82.112.255.157"; 
        
        private static int port = 8912;
        
        private static TranslationService _translationService = TranslationService.GetInstance(); 
        
        private static int timeoutMs = 3000;

        /// <summary>
        /// Establishes a TCP connection to a server with a specified timeout.
        /// </summary>
        /// <returns>
        /// A connected TcpClient instance if successful; otherwise, null.
        /// </returns>
        public static TcpClient Conn()
        {
            var client = new TcpClient();
            var cts = new CancellationTokenSource();

            var connectTask = Task.Run(() =>
            {
                try
                {
                    client.Connect(serverIp, port);
                }
                catch (Exception ex)
                {
                    Toastr.ShowServeurNotification($"🚨 {_translationService.GetText("Erreurréseau")} : {ex.Message}",BaseLayoutViewModel.Instance.NotificationContainer);
                }
            }, cts.Token);

            if (!connectTask.Wait(timeoutMs))
            {
                cts.Cancel();
                Toastr.ShowServeurNotification($"⚠️ {_translationService.GetText("Tempsdattentedépassé")}.",BaseLayoutViewModel.Instance.NotificationContainer);
                return null;
            }

            if (client.Connected)
            {
                Toastr.ShowServeurNotification($"✅ {_translationService.GetText("Connectéauserveur")} !",BaseLayoutViewModel.Instance.NotificationContainer);
                return client;
            }
            else
            {
                Toastr.ShowServeurNotification($"❌ {_translationService.GetText("Connexionéchouée")}.",BaseLayoutViewModel.Instance.NotificationContainer);
                return null;
            }
        }
    }
}