using System;
using System.Net.Sockets;
using SocksGateway.Models;

namespace SocksGateway.Socks
{
    public class SocksTunnel
    {
        public SocksTunnel(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;
        }

        public TcpClient Client { get; private set; }

        public void Open(string host, int port, ClientCredentials credentials = null)
        {
            var remoteClient = new SocksClient();
            remoteClient.Connect(host, port, credentials);
        }
    }
}