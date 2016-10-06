using SocksGateway.Socks.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocksGateway.Socks
{
    public class SocksServer
    {
        private const byte SOCKS_VERSION = 5;
        private TcpListener _listner;
        public bool IsRunning { get; private set; }

        public SocksServer(int port)
        {
            if (port <= 0 || port > 65535)
                throw new Exception("Unknown port");

            _listner = TcpListener.Create(port);
        }

        private async void WaitClients()
        {
            while (IsRunning)
            {
                var client = await _listner.AcceptTcpClientAsync();

                //event
                OnClientConnected(client);
            }
        }

        private Task WaitClientsTask()
        {
            return Task.Run(() => WaitClients());
        }

        private void OnClientConnected(TcpClient client)
        {
            ChoseAuthMerhod(client);
        }

        private byte[] ChoseAuthMerhod(TcpClient client)
        {
            var clientStream = client.GetStream();

            /* Client hello (3 bytes)
             * 1 - Version
             * 2 - Auth method number
             * 3 - Auth method
             */
            var clientBuffer = ReadClientData(clientStream, 3);

            if (clientBuffer[0] != SOCKS_VERSION)
                throw new Exception("Unknown protocol version.");

            var authMethod = (SocksAuthMethod)clientBuffer[2];

            /* Server hello (2 bytes)
            * 1 - Version
            * 2 - Auth method number
            * 3 - Auth method
            */
            var serverAnswer = new byte[2] { SOCKS_VERSION, (byte)authMethod };
            client.Client.Send(serverAnswer);

            if (authMethod == SocksAuthMethod.NotSupported)
                throw new Exception("Authentication method is not supported.");

            return null;
        }

        private byte[] AuthenticateByUsername(byte[] clientCredentials)
        {
            const int usernameStartPosition = 2;
            var usernameLength = clientCredentials[1];
            var usernameBytes = clientCredentials.Skip(usernameStartPosition).Take(usernameLength);
            var username = Encoding.UTF8.GetString(usernameBytes.ToArray());


            var passwordStartPosition = (usernameLength + usernameStartPosition) + 1;
            var passwordLength = clientCredentials[passwordStartPosition - 1];
            var passwordBytes = clientCredentials.Skip(passwordStartPosition).Take(passwordLength);
            var password = Encoding.UTF8.GetString(passwordBytes.ToArray());

            var serverResponseBuffer = new byte[2] { clientCredentials[0], 0 };
            if (username != Configuration.Username || password != Configuration.Password)
                serverResponseBuffer[1] = 0XFF;

            return serverResponseBuffer;
        }

        private byte[] ReadClientData(NetworkStream clientNetworkStream, int bufferSize)
        {
            var responseBuffer = new byte[bufferSize];
            clientNetworkStream.Read(responseBuffer, 0, bufferSize);
            return responseBuffer;
        }

        public void Start()
        {
            _listner.Start();
            IsRunning = true;
            WaitClientsTask();
        }

        public void Stop()
        {
            _listner.Stop();
            IsRunning = false;
        }
    }
}
