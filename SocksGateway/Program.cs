using System;
using SocksGateway.Socks;
using SocksGateway.Socks.Enums;

namespace SocksGateway
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var server = new SocksServer
            {
                Username = "admin",
                Password = "admin",
                IsProtected = true
            };
            server.Start();

            //var client = new SocksClient("39.1.42.161", 1080);
            //client.SendAuthMethod(AuthMethod.NoAuth);
            Console.ReadKey();
        }
    }
}