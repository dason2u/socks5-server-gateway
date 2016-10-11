using System;
using System.Net.Sockets;
using SocksGateway.Socks;
using SocksGateway.Socks.Events;
using SocksGateway.Socks.Helpers;

namespace SocksGateway
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RunSocksServer(true);
            //ClientRequest("127.0.0.1", 1080);

            Console.ReadKey();
        }

        #region Socks Client

        private static void ClientRequest(string host, int port)
        {
            var socksClient = new SocksClient();
            //socksClient.ClientCredentials = new ClientCredentials { Username = "admin", Password = "admin"};
            //socksClient.Connect(host, port);
        }

        #endregion

        private void Communicate(TcpClient client)
        {
            var clientRequestInfo = SocksServerHelpers.GetClientRequestInfo(client.GetStream());
            var proxyClient = new SocksClient();

            proxyClient.Connect(clientRequestInfo.Address, clientRequestInfo.Port);
            //proxyClient.
        }

        #region Socks Gateway

        private static void RunSocksServer(bool isProtected)
        {
            var server = new SocksServer
            {
                Username = "admin",
                Password = "admin",
                IsSecured = isProtected
            };

            server.OnClientAuthorized += OnSocksClientAuthorized;
            server.OnServerError += OnSocksServerError;
            server.OnHandshakeError += OnHandshakeError;

            server.Start();
        }

        private static void OnSocksClientAuthorized(object sender, SocksClientArgs e)
        {
        }

        private static void OnSocksServerError(object sender, SocksServerErrorArgs e)
        {
        }

        private static void OnHandshakeError(object sender, SocksErrorArgs e)
        {
        }

        #endregion
    }
}