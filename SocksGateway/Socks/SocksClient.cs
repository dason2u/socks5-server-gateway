using System;
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

        public string Address { get; set; }
        public int Port { get; set; }
        public ClientCredentials Credentials { get; set; }
        public bool Connected => _client.Connected;
            
        public SocksClient(string address, int port, ClientCredentials credentials = null)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Value cannot be null or empty.", nameof(address));

            Address = address;
            Port = port;
            Credentials = credentials;
            _client = new TcpClient();
        }

        public void Connect(string host, int port)
        {
            if(_client.Connected)
                return;

            _client.Connect(Address, Port);
            _clientStream = _client.GetStream();

            Handshake(Credentials);
            SocksClientHelpers.SendRequestDetails(_clientStream, host, port);
        }

        public void Send(byte[] data)
        {
            _clientStream.WriteAllData(data);
        }

        public byte[] Read(int bufferSize = 65536)
        {
            return _clientStream.ReadDataChunk(bufferSize);
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