using System;
using SocksGateway.Socks;
using SocksGateway.Socks.Events;

namespace SocksGateway
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RunSocksServer(true);

            Console.ReadKey();
        }

        #region Socks Gateway

        private static void RunSocksServer(bool isProtected)
        {
            var server = new SocksServer
            {
                Username = "admin",
                Password = "admin",
                IsProtected = isProtected
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