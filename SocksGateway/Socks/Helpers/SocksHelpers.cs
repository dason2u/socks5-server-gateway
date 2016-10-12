using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocksGateway.Models;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks.Helpers
{
    public static class SocksHelpers
    {
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