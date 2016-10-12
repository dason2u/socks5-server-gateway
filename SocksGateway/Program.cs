using System;
using System.Net.Sockets;
using SocksGateway.Socks;
using SocksGateway.Socks.Events;

namespace SocksGateway
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RunSocksServer();

            Console.ReadKey();
        }

        private static void OpenSocksTunnel(TcpClient client)
        {
            var socksTunnel = new SocksTunnel(client);
            socksTunnel.Open("108.61.164.110", 36982);
        }

        private static void RunSocksServer()
        {
            var socksServer = new SocksServer(Configuration.ServerPort)
            {
                Username = Configuration.ServerUsername,
                Password = Configuration.ServerPassword,
                IsSecured = Configuration.ServerIsSecured
            };
            socksServer.OnServerError += OnServerError;
            socksServer.OnClientHandshaked += OnClientHandshaked;
            socksServer.OnClientHandshakeError += OnClientHandshakeError;

            socksServer.Start();
        }

        private static void OnClientHandshaked(object sender, SocksClientArgs e)
        {
            OpenSocksTunnel(e.Client);
        }

        private static void OnClientHandshakeError(object sender, SocksClientErrorArgs e)
        {
        }

        private static void OnServerError(object sender, SocksServerErrorArgs e)
        {
        }
    }
}