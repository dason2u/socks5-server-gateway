using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SocksGateway.Models;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks.Helpers
{
    public static class SocksClientHelpers
    {
        public static void SendAuthMethod(NetworkStream clientStream, AuthMethod authMethod)
        {
            /* Send auth method (3 bytes)
             * 1 - Version
             * 2 - Auth method number
             * 3 - Auth method
             */
            var clientData = new byte[] {(byte) ProtocolVersion.V5, 0x01, (byte) authMethod};
            clientStream.WriteAllData(clientData);

            /* Server response chosen auth method (2 bytes)
            * 1 - Version
            * 2 - Chosen auth method
            */
            var serverResponse = clientStream.ReadDataChunk(2);
            var serverAuthMethod = serverResponse[1];

            if (serverAuthMethod != (byte) authMethod || serverAuthMethod == (byte) AuthMethod.NotSupported)
                throw new Exception("Authentication method not supported.");
        }

        public static void SendAuthCredentials(NetworkStream clientStream, ClientCredentials credentials)
        {
            /* Client credentials (unknown length)
            * 1 - Auth method version (0x01)
            * 2 - Username length
            * 3 - Username
            * 4 - Password length
            * 5 - Password
            */
            var credentialPackage = CreateCredentialsPackage(credentials);
            clientStream.WriteAllData(credentialPackage);

            /* Server response auth result (2 bytes)
            * 1 - Version
            * 2 - Result (0x00 - success)
            */
            var serverResponse = clientStream.ReadDataChunk(2);
            if(serverResponse[1] != 0)
                throw new Exception("Authentication error");
        }

        public static void SendRequest(NetworkStream clientStream, string host, int port)
        {
            var requestPackage = CreateRequestPackage(host, port);
            clientStream.WriteAllData(requestPackage);
        }

        #region Private Methods

        private static byte[] CreateCredentialsPackage(ClientCredentials credentials)
        {
            var usernameBytes = Encoding.UTF8.GetBytes(credentials.Username);
            var passwordBytes = Encoding.UTF8.GetBytes(credentials.Password);
            var credentialsPackage = new List<byte>
            {
                0x01,
                (byte)usernameBytes.Length
            };

            credentialsPackage.AddRange(usernameBytes);
            credentialsPackage.Add((byte) passwordBytes.Length);
            credentialsPackage.AddRange(passwordBytes);

            return credentialsPackage.ToArray();
        }

        private static byte[] CreateRequestPackage(string host, int port)
        {
            var requestPackage = new List<byte>
            {
                (byte) ProtocolVersion.V5,
                0x01,
                0x00
            };

            var addressBytes = Encoding.UTF8.GetBytes(host);

            var portBytes = BitConverter.GetBytes(port).Take(2).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portBytes);

            if (host.Count(x => x == '.') == 4)
            {
                requestPackage.Add((byte) AddressType.IPv4);
            }
            else
            {
                requestPackage.Add((byte) AddressType.Domain);
                requestPackage.Add((byte) addressBytes.Length);
            }

            requestPackage.AddRange(addressBytes);
            requestPackage.AddRange(portBytes);

            return requestPackage.ToArray();
        }

        #endregion
    }
}