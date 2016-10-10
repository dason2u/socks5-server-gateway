using System.Net.Sockets;
using SocksGateway.Models;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks
{
    public class SocksClient
    {
        private readonly TcpClient _client;
        private NetworkStream _clientStream;

        public SocksClient()
        {
            _client = new TcpClient();
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port);
            _clientStream = _client.GetStream();
        }

        public void Disconnect()
        {
            _client.Client.Disconnect(true);
        }

        private void Handshake(ClientCredentials credentials)
        {
            var authMethod = AuthMethod.NoAuth;
            SendAuthMethod(authMethod);
        }
    }
}