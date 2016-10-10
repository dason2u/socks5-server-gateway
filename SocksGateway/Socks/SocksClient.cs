using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks
{
    public class SocksClient
    {
        private TcpClient _client;
        private NetworkStream _clientStream;

        public SocksClient(string host, int port)
        {
            _client = new TcpClient();
            _client.Connect(host, port);
            _clientStream = _client.GetStream();
        }

        public void Handshake()
        {
            var authMethod = AuthMethod.NoAuth;
            SendAuthMethod(authMethod);

        }

        public void SendAuthMethod(AuthMethod authMethod)
        {
            /* Send auth method (3 bytes)
             * 1 - Version
             * 2 - Auth method number
             * 3 - Auth method
             */
            var clientRequest = new byte[] {(byte) ProtocolVersion.V5, 0x01, (byte) authMethod};
            SendData(clientRequest);

            /* Server send chosen auth method (2 bytes)
            * 1 - Version
            * 2 - Chosen auth method
            */
            var serverResponse = ReadData(2);
            var serverAuthMethod = serverResponse[1];
            if (serverAuthMethod != (byte)authMethod || serverAuthMethod == (byte)AuthMethod.NotSupported)
                throw new Exception("Authentication method not supported.");
        }

        #region Helpers

        private void SendData(byte[] data)
        {
            _clientStream.Write(data, 0, data.Length);
        }

        private byte[] ReadData(int bufferSize = 2048)
        {
            var buffer = new byte[bufferSize];
            var received = _clientStream.Read(buffer, 0, bufferSize);
            return buffer.Take(received).ToArray();
        }

        #endregion
    }
}
