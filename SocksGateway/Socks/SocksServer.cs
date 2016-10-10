using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SocksGateway.Socks.Enums;

namespace SocksGateway.Socks
{
    public class SocksServer
    {
        private readonly TcpListener _listener;
        public bool IsRunning { get; private set; }

        public bool IsProtected { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SocksServer(int port = 8080)
        {
            _listener = TcpListener.Create(port);
        }

        public void Start()
        {
            if(IsRunning)
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
                var client = await _listener.AcceptTcpClientAsync();
                Handshake(client);
            }
        }

        private Task WaitClientsTask()
        {
            return Task.Run(() => WaitClients());
        }

        private void Handshake(TcpClient client)  
        {
            var clientStream = client.GetStream();

            var authMethod = SocksServerHelper.ChoseAuthMethod(clientStream);
            var authenticated = AuthenticateClient(clientStream, authMethod);

            if (authenticated)
            {
                var clientRequestInfo = SocksServerHelper.GetClientRequestInfo(clientStream);


                clientRequestInfo.OriginalRequest[1] = 0x00;

                clientStream.Write(clientRequestInfo.OriginalRequest, 0, clientRequestInfo.OriginalRequest.Length);


                var message = Encoding.UTF8.GetBytes(@"HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Connection: close
Content-Length: 15

SOSI PISOS DURA");
                //if(client.Connected)
                clientStream.Write(message, 0, message.Length);
            }
        }

        #region Helpers

        private bool AuthenticateByUsernamePassword(NetworkStream clientStream)
        {
            var clientCredentials = SocksServerHelper.GetClientCredentials(clientStream);

            return clientCredentials.Username == Username && clientCredentials.Password == Password;
        }

        private bool AuthenticateClient(NetworkStream clientStream, AuthMethod authMethod)
        {
            bool valid;

            if(!IsProtected)
                authMethod = AuthMethod.NoAuth;

            SocksServerHelper.SendChosenAuthMethod(clientStream, authMethod);

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

            SocksServerHelper.SendAuthResult(clientStream, valid);
            return valid;
        }

        #endregion
    }
}
