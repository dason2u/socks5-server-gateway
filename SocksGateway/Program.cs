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
            RunSocksServer();

            Console.ReadKey();
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
            socksServer.OnHandshakeComplete += OnHandshakeComplete;
            socksServer.OnClientHandshakeError += OnClientHandshakeError;

            socksServer.Start();
        }

        private static void OpenSocksTunnel(TcpClient client)
        {
           SocksTunnel.PassDataViaTunnel(client);
        }

        #region Socks Server Events

        private static void OnHandshakeComplete(object sender, SocksClientArgs e)
        {
            Console.WriteLine($"Handshaking complete: {e.Client.Client.RemoteEndPoint}.");
            OpenSocksTunnel(e.Client);
        }

        private static void OnClientHandshakeError(object sender, SocksClientErrorArgs e)
        {
            Console.WriteLine(e.Exception.Message);
        }

        private static void OnServerError(object sender, SocksServerErrorArgs e)
        {
            Console.WriteLine(e.Exception.Message);
        }

        #endregion
    }
}