using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerUtils
    {
        public static string ReadMessage(User user)
        {
            byte[] buffer = new byte[8192];
            int bytesRead =  user.TcpClient.GetStream().Read(buffer, 0, buffer.Length);

            if (bytesRead <= 0)
            {
                return "";
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            WriteLog(user, message);
            
            return message;
        }
        
        public static void WriteLog(User user, string message)
        {
            string logFilePath = "log.txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {user.ToString()} - {message}");
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur lors de l'écriture dans le log : {e.Message}");
            }

            Console.WriteLine(message);
        }
    }
}