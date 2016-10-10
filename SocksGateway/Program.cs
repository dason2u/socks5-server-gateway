using System;
using SocksGateway.Socks;

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

            Console.ReadKey();
        }
    }
}