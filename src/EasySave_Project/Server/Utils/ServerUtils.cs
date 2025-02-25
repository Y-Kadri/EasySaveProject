using System.Text;
using Server.Models;

namespace Server.Utils
{
    public static class ServerUtils
    {
        /// <summary>
        /// Reads a message from the specified user's network stream.
        /// Logs the message and returns it as a string.
        /// </summary>
        /// <param name="user">The user whose message is being read.</param>
        /// <returns>The received message as a string, or an empty string if no data was read.</returns>
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
        
        /// <summary>
        /// Writes a log entry containing the user's details and the message they sent.
        /// </summary>
        /// <param name="user">The user associated with the log entry.</param>
        /// <param name="message">The message to log.</param>
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
        }
        
        /// <summary>
        /// Waits for a response from the specified user within a given timeout period.
        /// Checks for pending responses and returns the first available response.
        /// </summary>
        /// <param name="user">The user from whom a response is expected.</param>
        /// <param name="timeoutMs">The maximum time to wait for a response, in milliseconds.</param>
        /// <returns>The received response as a string, or null if no response is received within the timeout period.</returns>
        public static async Task<string?> WaitForResponse(User user, int timeoutMs)
        {
            int elapsedTime = 0;
            while (elapsedTime < timeoutMs)
            {
                lock (Server.pendingResponses)
                {
                    if (Server.pendingResponses.TryGetValue(user.Id, out string response))
                    {
                        Server.pendingResponses.Remove(user.Id);
                        return response;
                    }
                }
        
                await Task.Delay(100);
                elapsedTime += 100;
            }

            return null;
        }
    }
}