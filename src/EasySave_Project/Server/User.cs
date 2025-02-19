using System;
using System.Net.Sockets;

namespace Server
{
    public class User
    {
        public string Name { get; set; }
        public string Id { get; private set; }
        public string IPAddress { get; private set; }
        public int Port { get; private set; }
        public TcpClient TcpClient { get; private set; }

        public User(TcpClient client)
        {
            this.TcpClient = client;
            var endpoint = (System.Net.IPEndPoint)client.Client.RemoteEndPoint;
            this.IPAddress = endpoint.Address.ToString();
            this.Port = endpoint.Port;
            this.Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"{Id} - {IPAddress}:{Port}";
        }
    }
}