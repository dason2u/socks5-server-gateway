using System;
using System.Net.Sockets;
using SocksGateway.Socks.Helpers;

namespace SocksGateway.Socks
{
    public class SocksTunnel
    {
        public static void PassDataViaTunnel(TcpClient client)
        {
            var clientStream = client.GetStream();

            var clientRequestInfo = SocksTunnelHelpers.GetClientRequestInfo(clientStream);
            var proxyClient = new SocksClient("104.238.177.229", 40946);
            proxyClient.Connect(clientRequestInfo.Address, clientRequestInfo.Port);

            SocksTunnelHelpers.SendConnectResult(clientStream, true, clientRequestInfo.OriginalRequest);

            var clientData = clientStream.ReadDataChunk(65536);
            proxyClient.Send(clientData);

            var proxyClientResponse = proxyClient.Read();
            clientStream.WriteAllData(proxyClientResponse);
        }
    }
}