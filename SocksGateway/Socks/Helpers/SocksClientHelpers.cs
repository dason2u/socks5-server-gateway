using System;
using System.Net.Sockets;
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
    }
}