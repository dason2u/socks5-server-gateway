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
        public bool IsSecured { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public event EventHandler<SocksServerErrorArgs> OnServerError = delegate { };
        public event EventHandler<SocksClientErrorArgs> OnClientHandshakeError = delegate { };
        public event EventHandler<SocksClientArgs> OnHandshakeComplete = delegate { };

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

        #region Authentication Methods

        private bool AuthenticateByUsernamePassword(NetworkStream clientStream)
        {
            var clientCredentials = SocksServerHelpers.GetClientCredentials(clientStream);
            return (clientCredentials.Username == Username) && (clientCredentials.Password == Password);
        }

        #endregion

        #region Private Methods

        private Task WaitClientsTask()
        {
            return Task.Run(() => WaitClients());
        }

        private async void WaitClients()
        {
            while (IsRunning)
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();

#pragma warning disable 4014
                    HandshakeTask(client)
                        .ContinueWith(task => HandshakeEnded(task, client));
#pragma warning restore 4014
                }
                catch (Exception e)
                {
                    OnServerError(this, new SocksServerErrorArgs(e));
                }
        }

        private Task HandshakeTask(TcpClient client)
        {
            return Task.Run(() => Handshake(client));
        }

        private void Handshake(TcpClient client)
        {
            var clientStream = client.GetStream();

            var authMethod = SocksServerHelpers.GetAuthMethod(clientStream);
            var authenticated = AuthenticateClient(clientStream, authMethod);

            if (!authenticated)
                throw new Exception("Authentication error.");
        }

        private void HandshakeEnded(Task handshakeTask, TcpClient client)
        {
            if (handshakeTask.IsFaulted)
                OnClientHandshakeError(this, new SocksClientErrorArgs(client, handshakeTask.Exception));
            else
                OnHandshakeComplete(this, new SocksClientArgs(client));
        }

        private bool AuthenticateClient(NetworkStream clientStream, AuthMethod authMethod)
        {
            bool valid;

            if (!IsSecured)
                authMethod = AuthMethod.NoAuth;

            SocksServerHelpers.SendChosenAuthMethod(clientStream, authMethod);

            switch (authMethod)
            {
                case AuthMethod.NoAuth:
                    return true;
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

        #endregion
    }
}