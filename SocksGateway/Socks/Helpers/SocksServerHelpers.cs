using System;
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

            if (!Enum.IsDefined(typeof(AuthMethod), authMethod) || (authMethod == AuthMethod.NotSupported))
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

        #region Private Methods

        private static ClientCredentials ParseClientCredentials(byte[] clientResponse)
        {
            var usernameLength = Convert.ToInt32(clientResponse[1]);
            var passwordLength = Convert.ToInt32(clientResponse[usernameLength + 2]);
            var username = Encoding.ASCII.GetString(clientResponse, 2, usernameLength);
            var password = Encoding.ASCII.GetString(clientResponse, usernameLength + 3, passwordLength);

            return new ClientCredentials {Username = username, Password = password};
        }

        #endregion
    }
}