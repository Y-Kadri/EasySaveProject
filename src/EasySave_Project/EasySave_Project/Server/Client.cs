using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.Views.Components;

namespace EasySave_Project.Server
{
    public class Client
    {
        public TcpClient client;
        public NetworkStream stream { get; }
        public string name;

        public Client(string name)
        {
            try
            {
                // Utilise la méthode asynchrone pour la connexion avec timeout
                client = Connexion.Conn();
                if (client == null) 
                {
                    Toastr.ShowServeurNotification("❌ Connexion échouée.",BaseLayoutViewModel.Instance.NotificationContainer);
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
                Toastr.ShowServeurNotification($"⚠️ Impossible de se connecter au serveur : {ex.Message}",BaseLayoutViewModel.Instance.NotificationContainer);
            }
        }

        public void Start()
        {
            // Reste de ton code (ou vide)
        }
    }
}