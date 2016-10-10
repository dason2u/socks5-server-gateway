using System;
using System.Net.Sockets;

namespace SocksGateway.Socks.Events
{
    public class SocksClientArgs
    {
        public SocksClientArgs(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;
        }

        public TcpClient Client { get; set; }
    }
}