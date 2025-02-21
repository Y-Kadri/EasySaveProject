using System.Net.Sockets;

namespace EasySave_Project.Server
{
    public class Connexion
    {
        private static string serverIp = "82.112.255.157"; 
        private static int port = 8912;

        public static TcpClient Conn()
        {
            return new TcpClient(serverIp, port);
        }
    }
}