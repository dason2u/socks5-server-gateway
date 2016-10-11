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

        public SocksClient()
        {
            _client = new TcpClient();
        }

        public void Connect(string host, int port, ClientCredentials credentials = null)
        {
            _client.Connect(host, port);
            _clientStream = _client.GetStream();

            Handshake(credentials);
        }

        public void Disconnect()
        {
            _client.Client.Disconnect(true);
        }

        #region Private Methods

        private void Handshake(ClientCredentials credentials)
        {
            if (credentials == null)
            {
                SocksClientHelpers.SendAuthMethod(_clientStream, AuthMethod.NoAuth);
            }
            else
            {
                SocksClientHelpers.SendAuthMethod(_clientStream, AuthMethod.UsernamePassword);
                SocksClientHelpers.SendAuthCredentials(_clientStream, credentials);
            }
        }

        #endregion
    }
}