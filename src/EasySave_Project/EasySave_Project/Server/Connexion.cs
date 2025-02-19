using System.Net.Sockets;

namespace EasySave_Project.Server
{
    public class Connexion
    {
        private static string serverIp = "127.0.0.1"; 
        private static int port = 8080;

        public static TcpClient Conn()
        {
            return new TcpClient(serverIp, port);
        }
    }
}