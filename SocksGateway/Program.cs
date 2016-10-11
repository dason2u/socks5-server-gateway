using System;
using SocksGateway.Models;
using SocksGateway.Socks;
using SocksGateway.Socks.Events;

namespace SocksGateway
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //RunSocksServer(true);
            ClientRequest("127.0.0.1", 1080);

            Console.ReadKey();
        }

        #region Socks Client

        private static void ClientRequest(string host, int port)
        {
            var socksClient = new SocksClient();
            socksClient.ClientCredentials = new ClientCredentials { Username = "admin", Password = "admin"};
            socksClient.Connect(host, port);
        }

        #endregion

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
            server.OnClientConnected += OnSocksClientConnected;
            server.OnClientDisconnected += OnSocksClientDisconnected;
            server.OnError += OnSocksServerError;

            server.Start();
        }

        private static void OnSocksClientDisconnected(object sender, SocksClientArgs e)
        {
        }

        private static void OnSocksClientConnected(object sender, SocksClientArgs e)
        {
        }

        private static void OnSocksClientAuthorized(object sender, SocksClientArgs e)
        {
        }

        private static void OnSocksServerError(object sender, SocksErrorArgs e)
        {
        }

        #endregion
    }
}