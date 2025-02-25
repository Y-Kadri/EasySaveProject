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
    public class Connexion
    {
        // private static string serverIp = "127.0.0.1"; 
        // private static int port = 8080;
        
        private static string serverIp = "82.112.255.157"; 
        private static int port = 8912;
        private static TranslationService _translationService = TranslationService.GetInstance(); 
        
        private static int timeoutMs = 3000; // ⏳ Timeout de 3s

        public static TcpClient Conn()
        {
            var client = new TcpClient();
            var cts = new CancellationTokenSource();

            // Tente de se connecter en arrière-plan
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

            // ⏳ Attente max de 3 secondes
            if (!connectTask.Wait(timeoutMs))
            {
                cts.Cancel();
                Toastr.ShowServeurNotification($"⚠️ {_translationService.GetText("Tempsdattentedépassé")}.",BaseLayoutViewModel.Instance.NotificationContainer);
                return null; // ⛔ Retourne `null` si timeout
            }

            // ✅ Connexion réussie
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