using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerUtils
    {
        public static string ReadMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[8192];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead <= 0)
            {
                return "";
            }

            return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }
    }
}