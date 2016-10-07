using SocksGateway.Socks.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SocksGateway.Models;

namespace SocksGateway.Socks
{
    public class SocksServer
    {
        private const byte SOCKS_VERSION = 5;
        private TcpListener _listner;
        public bool IsRunning { get; private set; }
        public bool IsProtected { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }

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

        private Task WaitClientsTask()
        {
            return Task.Run(() => WaitClients());
        }

        private void OnClientConnected(TcpClient client)
        {
            ChoseAuthMerhod(client);
        }

        private void ChoseAuthMerhod(TcpClient client)
        {
            var clientStream = client.GetStream();
            var authMethod = GetClientAuthMethod(clientStream);
            var isAuthenticated = AuthenticateClient(clientStream, authMethod);

            if (!isAuthenticated)
                throw new Exception("Authentication failed.");

            var clientAddress = GetClientRequestInfo(clientStream);
        }

        #region Handshaking

        private bool AuthenticateClient(NetworkStream clientStream, SocksAuthMethod authMethod)
        {
            /* Server send chosen auth method (2 bytes)
            * 1 - Version
            * 2 - Chosen auth method
            */
            var serverAnswer = new[] { SOCKS_VERSION, (byte)authMethod };
            clientStream.Write(serverAnswer, 0, serverAnswer.Length);

            var isAuthenticated = Authenticate(clientStream, authMethod);

            if (isAuthenticated)
            {
                /* Server authenticated response (2 bytes)
                 * 1 - Version
                 * 2 - IsAuthenticated (0x00 - Success)
                 */
                var successAuthResponse = new byte[] { SOCKS_VERSION, 0x00 };
                clientStream.Write(successAuthResponse, 0, successAuthResponse.Length);
            }

            return isAuthenticated;
        }

        private bool AuthenticateByUsernameAndPassword(NetworkStream clientStream)
        {
            /* Client credentials (unknown length)
             * 1 - Version
             * 2 - Username length
             * 3 - Username
             * 4 - Password length
             * 5 - Password
             */
            var clientCredsPacket = ReadClientData(clientStream);
            var clientCreds = ParseClientCredentials(clientCredsPacket);

            return (clientCreds.Username == Username && clientCreds.Password == Password);
        }

        private SocksAuthMethod GetClientAuthMethod(NetworkStream clientStream)
        {
            /* Client hello (3 bytes)
             * 1 - Version
             * 2 - Auth method number
             * 3 - Auth method
             */
            var clientResponse = ReadClientData(clientStream, 3);

            if (clientResponse[0] != SOCKS_VERSION)
                throw new Exception("Unknown protocol version");

            var authMethod = (SocksAuthMethod)clientResponse[2];

            if (IsProtected && authMethod != SocksAuthMethod.UsernamePassword)
                throw new Exception("Authentication required.");

            return authMethod;
        }

        private ClientRequestInfo GetClientRequestInfo(NetworkStream clientStream)
        {
            /* Client request info (unknown length)
             * 1 - Version
             * 2 - Command
             * 3 - Reserved (0x00)
             * 4 - Address type
             * 5 - Destination address
             * 6 - Destination port
             */
            var clientBytes = ReadClientData(clientStream);

            if (clientBytes[1] != 1)
                throw new Exception("Unsupported request command.");

            var address = ParseRequestAddress(clientBytes);
            return null;
        }

        #endregion

        #region Helpers

        private ClientCredentials ParseClientCredentials(byte[] clientResponse)
        {
            var usernameLength = Convert.ToInt32(clientResponse[1]);
            var passwordLength = Convert.ToInt32(clientResponse[usernameLength + 2]);
            var username = Encoding.ASCII.GetString(clientResponse, 2, usernameLength);
            var password = Encoding.ASCII.GetString(clientResponse, usernameLength + 3, passwordLength);

            return new ClientCredentials { Username = username, Password = password };
        }

        private string ParseRequestAddress(byte[] clientResponse)
        {
            var addressType = (AddressType)clientResponse[3];

            string address;
            int portStartPosition = 4;
            switch (addressType)
            {
                case AddressType.IPv4:
                    var ipAddressBytes = clientResponse.Skip(4).Take(4).ToArray();
                    address = new IPAddress(ipAddressBytes).ToString();
                    portStartPosition += 4; //IP address length
                    break;
                case AddressType.Domain:
                    int domainLength = Convert.ToInt32(clientResponse[4]);
                    address = Encoding.ASCII.GetString(clientResponse, 5, domainLength);
                    portStartPosition += domainLength + 1; //Domain data plus domain length element
                    break;
                default:
                    throw new Exception("Unknown address type.");
            }

            //var port = clientResponse.Take(clientResponse.Length - 2).ToArray();
            var port = BitConverter.ToInt32(clientResponse, clientResponse.Length - 3);
            //if(port < 1 || port > 65535)
            return address;
        }

        private bool Authenticate(NetworkStream clientStream, SocksAuthMethod authMethod)
        {
            switch (authMethod)
            {
                case SocksAuthMethod.NoAuth:
                    return true;
                case SocksAuthMethod.UsernamePassword:
                    return AuthenticateByUsernameAndPassword(clientStream);
                default:
                    return false;
            }
        }

        private byte[] ReadClientData(NetworkStream clientStream, int bufferSize = 65535)
        {
            var responseBuffer = new byte[bufferSize];
            var bytesReaded = clientStream.Read(responseBuffer, 0, bufferSize);
            return responseBuffer.Take(bytesReaded).ToArray();
        }

        #endregion
    }
}
