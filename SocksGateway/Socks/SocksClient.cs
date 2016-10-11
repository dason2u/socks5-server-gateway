using System.Net.Sockets;
using SocksGateway.Models;
using SocksGateway.Socks.Enums;
using SocksGateway.Socks.Helpers;

namespace SocksGateway.Socks
{
    public class SocksClient
    {
        private readonly TcpClient _client;
        private NetworkStream _clientStream;

        public ClientCredentials ClientCredentials { get; set; }

        public SocksClient()
        {
            _client = new TcpClient();
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port);
            _clientStream = _client.GetStream();

            Handshake();
        }

        public void Disconnect()
        {
            _client.Client.Disconnect(true);
        }

        private void Handshake()
        {
            Authentication();
            SocksClientHelpers.SendRequest(_clientStream, "www.google.com", 80);

            var data = _clientStream.ReadDataChunk();
        }

        private void Authentication()
        {
            if (ClientCredentials == null)
            {
                SocksClientHelpers.SendAuthMethod(_clientStream, AuthMethod.NoAuth);
            }
            else
            {
                SocksClientHelpers.SendAuthMethod(_clientStream, AuthMethod.UsernamePassword);
                SocksClientHelpers.SendAuthCredentials(_clientStream, ClientCredentials);
            }
        }
    }
}