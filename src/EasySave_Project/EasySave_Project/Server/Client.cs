using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.Views.Components;

namespace EasySave_Project.Server
{
    public class Client
    {
        public TcpClient client;
        public NetworkStream stream { get; }
        
        public string name;
        
        private static readonly TranslationService _translationService = TranslationService.GetInstance();

        public Client(string name)
        {
            try
            {
                client = Connexion.Conn();
                if (client == null) 
                {
                    Toastr.ShowServeurNotification($"❌ {_translationService.GetText("Connexionéchouée")}.",BaseLayoutViewModel.Instance.NotificationContainer);
                    throw new Exception();
                }
                stream = client.GetStream();
                this.name = name;
                Utils.SendToServer(name, stream);

                Thread receiveThread = new Thread(() => Utils.StartListening(stream))
                {
                    IsBackground = true
                };
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Toastr.ShowServeurNotification($"⚠️ {_translationService.GetText("Tempsdattentedépassé")} : {ex.Message}",BaseLayoutViewModel.Instance.NotificationContainer);
            }
        }
        
    }
}