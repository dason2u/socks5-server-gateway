using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocksGateway.Models;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks.Helpers
{
    public static class SocksServerHelpers
    {
        public static AuthMethod GetAuthMethod(NetworkStream clientStream)
        {
            /* Client hello (3 bytes)
             * 1 - Version
             * 2 - Auth method number
             * 3 - Auth method
             */
            var clientResponse = clientStream.ReadDataChunk(3);

            if (clientResponse[0] != (byte) ProtocolVersion.V5)
                throw new Exception("Unknown protocol version");

            var authMethod = (AuthMethod) clientResponse[2];

            if (!Enum.IsDefined(typeof (AuthMethod), authMethod) || authMethod == AuthMethod.NotSupported)
                throw new Exception("Authentication method is not supported.");

            return authMethod;
        }

        public static void SendChosenAuthMethod(NetworkStream clientStream, AuthMethod authMethod)
        {
            /* Server send chosen auth method (2 bytes)
            * 1 - Version
            * 2 - Chosen auth method
            */
            var serverData = new[] {(byte) ProtocolVersion.V5, (byte) authMethod};
            clientStream.WriteAllData(serverData);
        }

        public static ClientCredentials GetClientCredentials(NetworkStream clientStream)
        {
            /* Client credentials (unknown length)
            * 1 - Auth method version
            * 2 - Username length
            * 3 - Username
            * 4 - Password length
            * 5 - Password
            */
            var clientResponse = clientStream.ReadDataChunk(8192);
            return ParseClientCredentials(clientResponse);
        }

        public static void SendAuthResult(NetworkStream clientStream, bool authenticated)
        {
            /* Server authenticated response (2 bytes)
            * 1 - Version
            * 2 - IsAuthenticated (0x00 - Success)
            */
            var serverData = new byte[] {(byte) ProtocolVersion.V5, 0x00};
            if (!authenticated)
                serverData[1] = 0xFF;

            clientStream.WriteAllData(serverData);
        }

        public static SocksRequestInfo GetClientRequestInfo(NetworkStream clientStream)
        {
            /* Client request info (unknown length)
             * 1 - Version
             * 2 - Command
             * 3 - Reserved (0x00)
             * 4 - Address type
             * 5 - Destination address
             * 6 - Destination port
             */
            var clientResponse = clientStream.ReadDataChunk();

            if (clientResponse[1] != 1)
                throw new Exception("Unsupported request command.");

            return ParseHostInfo(clientResponse);
        }

        #region Private Methods

        private static ClientCredentials ParseClientCredentials(byte[] clientResponse)
        {
            var usernameLength = Convert.ToInt32(clientResponse[1]);
            var passwordLength = Convert.ToInt32(clientResponse[usernameLength + 2]);
            var username = Encoding.ASCII.GetString(clientResponse, 2, usernameLength);
            var password = Encoding.ASCII.GetString(clientResponse, usernameLength + 3, passwordLength);

            return new ClientCredentials {Username = username, Password = password};
        }

        private static SocksRequestInfo ParseHostInfo(byte[] clientResponse)
        {
            var addressType = (AddressType) clientResponse[3];

            string address;
            switch (addressType)
            {
                case AddressType.IPv4:
                    var ipAddressBytes = clientResponse.Skip(4).Take(4).ToArray();
                    address = new IPAddress(ipAddressBytes).ToString();
                    break;
                case AddressType.Domain:
                    var domainLength = Convert.ToInt32(clientResponse[4]);
                    address = Encoding.ASCII.GetString(clientResponse, 5, domainLength);
                    break;
                default:
                    throw new Exception("Unknown address type.");
            }

            //Little endian byte port to int
            var portBuffer = new[] {clientResponse.Last(), clientResponse[clientResponse.Length - 2]};
            var port = BitConverter.ToUInt16(portBuffer, 0);

            return new SocksRequestInfo {Address = address, Port = port, OriginalRequest = clientResponse};
        }

        #endregion
    }
}