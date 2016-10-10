using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SocksGateway.Socks.Enums;
using SocksGateway.Socks.Events;
using SocksGateway.Socks.Helpers;

namespace SocksGateway.Socks
{
    public class SocksServer
    {
        private readonly TcpListener _listener;

        public SocksServer(int port = 8080)
        {
            _listener = TcpListener.Create(port);
        }

        public bool IsRunning { get; private set; }
        public bool IsProtected { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public event EventHandler<SocksErrorArgs> OnError = delegate { };
        public event EventHandler<SocksClientArgs> OnClientConnected = delegate { };
        public event EventHandler<SocksClientArgs> OnClientAuthorized = delegate { };
        public event EventHandler<SocksClientArgs> OnClientDisconnected = delegate { };

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            _listener.Start();
            WaitClientsTask();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            _listener.Stop();
        }

        private async void WaitClients()
        {
            while (IsRunning)
            {
                TcpClient client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                    OnClientConnected(this, new SocksClientArgs(client));
                    Handshake(client);
                }
                catch (Exception e)
                {
                    OnError(this, new SocksErrorArgs(client, e));

                    if (client != null)
                    {
                        client.Dispose();
                        OnClientDisconnected(this, new SocksClientArgs(client));
                    }
                }
            }
        }

        private Task WaitClientsTask()
        {
            return Task.Run(() => WaitClients());
        }

        private void Handshake(TcpClient client)
        {
            var clientStream = client.GetStream();

            var authMethod = SocksServerHelpers.GetAuthMethod(clientStream);
            var authenticated = AuthenticateClient(clientStream, authMethod);

            if (!authenticated)
                throw new Exception("Authentication error.");

            OnClientAuthorized(this, new SocksClientArgs(client));
        }

        private bool AuthenticateClient(NetworkStream clientStream, AuthMethod authMethod)
        {
            bool valid;

            if (!IsProtected)
                authMethod = AuthMethod.NoAuth;

            SocksServerHelpers.SendChosenAuthMethod(clientStream, authMethod);

            switch (authMethod)
            {
                case AuthMethod.NoAuth:
                    valid = true;
                    break;
                case AuthMethod.UsernamePassword:
                    valid = AuthenticateByUsernamePassword(clientStream);
                    break;
                default:
                    valid = false;
                    break;
            }

            SocksServerHelpers.SendAuthResult(clientStream, valid);
            return valid;
        }

        #region Authentication Methods

        private bool AuthenticateByUsernamePassword(NetworkStream clientStream)
        {
            var clientCredentials = SocksServerHelpers.GetClientCredentials(clientStream);
            return (clientCredentials.Username == Username) && (clientCredentials.Password == Password);
        }

        #endregion
    }
}